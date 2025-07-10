using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Newtonsoft.Json.Linq;

namespace CargoWise.Macros.GUI
{
	#region SuppressResourceStringsCheckRegion

	[DesignerSerializer(typeof(Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class MacroEvaluationForm : Form
	{
		public MacroEvaluationForm(IMacroEvaluationContext context, string input = null, IMacroScope scope = null, bool persistSettings = true)
			: this()
		{
			this.persistSettings = persistSettings;
			this.scope = scope;
			this.context = context ?? new MacroEvaluationContext();

			Init(input);
		}

		// please do not remove, needed for VS designer
		MacroEvaluationForm()
		{
			InitializeComponent();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		readonly IMacroScope scope;
		readonly IMacroEvaluationContext context;
		readonly bool persistSettings;

		const string settingsFileName = "macroevalform.xml";
		readonly string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Macros");

		const int WM_KEYDOWN = 0x100;

		void Init(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				TryLoadLastInput();
			}
			else
			{
				inputTextBox.Text = input;
				inputTextBox.SelectionStart = 0;
				inputTextBox.SelectionLength = 0;
			}

			inputTextBox.HideSelection = false;

			runButton.Focus();

			runButton.Click += (s, e) => Run(Input);
			informationsTabControl.SelectedIndexChanged += (s, e) => UpdateVisibleTab();
		}

		void UpdateVisibleTab()
		{
			if (informationsTabControl.SelectedTab == astTabPage)
			{
				PrintAST(ast);
			}
			else if (informationsTabControl.SelectedTab == expressionTabPage)
			{
				PrintExpression(expression);
			}
		}

		#region Settings
		#region SuppressResourceStringsCheckRegion

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This function has supported out of screen processing")]
		void TryLoadLastInput()
		{
			var settingsFilePath = Path.Combine(settingsDirectory, settingsFileName);

			if (File.Exists(settingsFilePath))
			{
				try
				{
					var doc = XDocument.Load(settingsFilePath);

					if (doc.Root == null)
					{
						return;
					}

					var elem = doc.Root.Element("Input");

					if (!string.IsNullOrEmpty(elem?.Value))
					{
						inputTextBox.Text = elem.Value.Replace("\n", Environment.NewLine);
					}

					elem = doc.Root.Element("SelectionStart");

					inputTextBox.SelectionStart = elem != null
						? Convert.ToInt32(elem.Value, CultureInfo.InvariantCulture)
						: 0;

					elem = doc.Root.Element("SelectionLength");

					inputTextBox.SelectionLength = elem != null
						? Convert.ToInt32(elem.Value, CultureInfo.InvariantCulture)
						: 0;
				}
				catch (System.Security.SecurityException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
		}

		void TryWriteLastInput()
		{
			if (!persistSettings || string.IsNullOrWhiteSpace(inputTextBox.Text))
			{
				return;
			}

			if (!Directory.Exists(settingsDirectory))
			{
				Directory.CreateDirectory(settingsDirectory);
			}

			var settingsFilePath = Path.Combine(settingsDirectory, settingsFileName);

			try
			{
				var doc = new XDocument(
					new XElement("ASTViewSettings",
						new XElement("Input", inputTextBox.Text),
						new XElement("SelectionStart", inputTextBox.SelectionStart),
						new XElement("SelectionLength", inputTextBox.SelectionLength)));

				doc.Save(settingsFilePath);
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		#endregion
		#endregion

		string Input
		{
			get
			{
				var selectedText = inputTextBox.SelectedText;

				return !string.IsNullOrWhiteSpace(selectedText)
					? selectedText
					: inputTextBox.Text;
			}
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == WM_KEYDOWN)
			{
				#pragma warning disable WFDEV001 // 'Message.WParam' is obsolete: 'Casting to/from IntPtr is unsafe, use WParamInternal.' 
				if ((Keys)m.WParam == Keys.F5)
				#pragma warning restore WFDEV001 // Restore warning checking
				{
					Run(Input);
					return true;
				}
			}

			return base.ProcessKeyPreview(ref m);
		}

		IMacroExpression expression;
		SyntaxNode ast;

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Developer tools")]
		void Run(string input)
		{
			runButton.Visible = false;
			loaderPictureBox.Visible = true;
			resultsTextBox.Text = string.Empty;
			variablesTextBox.Text = string.Empty;
			performanceTextBox.Text = string.Empty;

			System.Windows.Forms.Application.DoEvents();

			TryWriteLastInput();

			var timeRecorder = new TimeRecorder();

			expression = Compile(timeRecorder, input);

			if (expression != null && !expression.Errors.Any())
			{
				Run(timeRecorder, expression);
			}

			LogOutput(performanceTextBox, string.Format(CultureInfo.InvariantCulture, "Total time:       {0:g}", timeRecorder.TotalRecorded));

			runButton.Visible = true;
			loaderPictureBox.Visible = false;
		}

		IMacroExpression Compile(TimeRecorder timeRecorder, string input)
		{
			ast = null;

			var expr = input
				.With(context)
				.CreateExpression();

			using (timeRecorder.Record())
			{
				ast = expr.AST;
			}

			LogOutput(performanceTextBox, string.Format(CultureInfo.InvariantCulture, "Compilation time: {0}", timeRecorder.Elapsed.ToString("g", CultureInfo.InvariantCulture)));

			var compilationErrors = expr.Errors
				.Where(error => error.Category == ErrorCategory.Compiletime)
				.ToArray();

			if (compilationErrors.Any())
			{
				LogOutput(resultsTextBox, "The following errors have occurred during compilation:");

				foreach (var error in compilationErrors)
				{
					LogOutput(resultsTextBox, error.Message);
				}

				LogOutput(resultsTextBox, string.Empty);
			}

			return expr;
		}

		void Run(TimeRecorder timeRecorder, IMacroExpression expr)
		{
			object result = null;

			using (var runScope = new MacroScope(scope))
			{
				using (timeRecorder.Record())
				{
					result = ObjectFactory.Get<IMacroEvaluator>().EvaluateMacroValue(scope, expr);
				}

				foreach (var variable in runScope.GetVariables())
				{
					LogOutput(variablesTextBox, variable.Key);
				}
			}

			LogOutput(resultsTextBox, ConvertForDisplay(result));

			var runtimeErrors = expr
				.Errors
				.Where(error => error.Category == ErrorCategory.Runtime)
				.ToArray();

			if (runtimeErrors.Any())
			{
				LogOutput(resultsTextBox, "The following errors have occurred during evaluation:");

				foreach (var error in runtimeErrors)
				{
					LogOutput(resultsTextBox, error.Message);
				}
			}
		}

		string ConvertForDisplay(object obj)
		{
			if (obj == null)
			{
				return "[null]";
			}
			int depth = 5;

			// handling for CW1 business objects will be added later
			if (obj.GetType().GetInterface("IBusiness", true) != null)
			{
				return obj.ToString();
			}

			var value = obj.ToMapValue(depth);

			var map = value as MacroMap;

			if (map != null)
			{
				var json = map.ToJSON(depth);
				var jsonObj = JObject.Parse(json);

				return jsonObj.ToString();
			}

			var list = value as IEnumerable<object>;

			if (list != null)
			{
				var json = list.ToJSON(depth);
				var jsonArray = JArray.Parse(json);

				return jsonArray.ToString();
			}

			if (value != null)
			{
				return Convert.ToString(value, CultureInfo.InvariantCulture);
			}

			return Convert.ToString(obj, CultureInfo.InvariantCulture);
		}

		void PrintAST(SyntaxNode astNode)
		{
			astTextBox.Text = string.Empty;

			if (astNode != null)
			{
				PrintAST(astNode, astTextBox);
			}
		}

		void PrintAST(SyntaxNode node, KTextBox textBox, int level = 0)
		{
			var indent = string.Empty;

			if (level == 1)
			{
				indent = "|-";
			}
			else if (level > 1)
			{
				indent = string.Concat(new string(' ', level * 2), "|-");
			}

			var nodeText = string.Empty;

			if (node == null)
			{
				nodeText = "NULL";
			}
			else if (node.Value != null)
			{
				nodeText = string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", node.Type, node.Value);
			}
			else
			{
				nodeText = node.Type.ToString();
			}

			LogOutput(textBox, string.Concat(indent, nodeText));

			foreach (var child in node.ChildNodes)
			{
				PrintAST(child, textBox, level + 1);
			}
		}

		void PrintExpression(IMacroExpression expr)
		{
			expressionTextBox.Text = string.Empty;

			if (expr?.Expression != null)
			{
				LogOutput(expressionTextBox, "AST has been compiled to:");
				LogOutput(expressionTextBox, expr.Expression.ToDebugString());
			}
		}

		void LogOutput(KTextBox textBox, params string[] lines)
		{
			if (lines == null || lines.Length == 0)
			{
				return;
			}

			textBox.Visible = true;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(textBox.Text))
				{
					textBox.Text += line;
				}
				else
				{
					textBox.Text += string.Concat(Environment.NewLine, line);
				}
			}

			textBox.SelectionStart = textBox.Text.Length;
			textBox.ScrollToCaret();
		}
	}

	#endregion
}
