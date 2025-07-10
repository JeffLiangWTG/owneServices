using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class ExpressionNoteTemplate : StmNoteTemplate
	{
		public ExpressionNoteTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string Placeholder = "___"; // Hard-coded constant
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded delimiter")]
		const string Delimiter = "◄◘►";

		[ChildEditable]
		public ExpressionPlaceholderCollection Placeholders
		{
			get
			{
				if (placeholders == null)
				{
					placeholders = new ExpressionPlaceholderCollection();
					RegisterEditableChildObject(placeholders);
				}

				return placeholders;
			}
		}
		ExpressionPlaceholderCollection placeholders;

		public override ZString TemplateText
		{
			get { return templateText; }
			set
			{
				if (value != templateText)
				{
					HasChanges = true;
					templateText = value;
					ResizePlaceholders();
				}
			}
		}
		ZString templateText;

		void ResizePlaceholders()
		{
			var newPlaceholders = templateText.ToString().Split(new string[] { Placeholder }, StringSplitOptions.None).Length - 1;
			var existingPlaceholders = Placeholders.Count;

			if (newPlaceholders > existingPlaceholders)
			{
				for (int i = 0; i < newPlaceholders - existingPlaceholders; i++)
				{
					var expressionPlaceholder = new ExpressionPlaceholder(i + existingPlaceholders + 1);
					Placeholders.Add(expressionPlaceholder);
				}
			}
			else if (existingPlaceholders > newPlaceholders)
			{
				for (int i = 0; i < existingPlaceholders - newPlaceholders; i++)
				{
					Placeholders.Remove(Placeholders[Placeholders.Count - 1]);
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			var templateInfo = S8_TemplateText.ToString().Split(new string[] { Delimiter }, StringSplitOptions.None);
			templateText = templateInfo[0];
			ResizePlaceholders();

			for (int i = 1; i < templateInfo.Length; i++)
			{
				Placeholders[i - 1].Description = templateInfo[i];
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			UpdateTemplateText();
			base.RunPreSaveValidationCore();
		}

		public override void OnSaving()
		{
			UpdateTemplateText();
			base.OnSaving();
		}

		void UpdateTemplateText()
		{
			S8_TemplateText = Placeholders.Cast<ExpressionPlaceholder>().Aggregate(TemplateText, (result, placeholder) => result + Delimiter + placeholder.Description);
		}

		public ZString Replacement
		{
			get
			{
				var result = ZString.Empty;
				var sections = TemplateText.ToString().Split(new string[] { Placeholder }, StringSplitOptions.None);

				for (int i = 0; i < Placeholders.Count; i++)
				{
					result += sections[i];
					var replacement = Placeholders[i].Replacement;
					result += !replacement.IsEmpty ? replacement.ToString() : Placeholder;
				}
				result += sections[sections.Length - 1];

				return result;
			}
		}
	}
}
