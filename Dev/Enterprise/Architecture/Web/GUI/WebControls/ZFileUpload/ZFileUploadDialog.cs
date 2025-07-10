using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Brettle.Web.NeatUpload;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZFileUploadDialog : ZIFramePage
	{
		#region Constructors

		public ZFileUploadDialog()
			: base()
		{
		}

		#endregion

		#region Overrides

		protected override void OnPreRender(EventArgs e)
		{
			NotificationFlags.DisplayAll = false;
			NotificationFlags.DisplayErrors = false;
			NotificationFlags.DisplayMessageErrors = false;
			NotificationFlags.DisplayWarnings = false;
			base.OnPreRender(e);
		}

		protected override string GetOKButtonWidth()
		{
			return "120px";
		}

		protected override void HandleOkButtonClick()
		{
			if (ProceedWithUpload())
			{
				var data = GetUploadedFileData();
				if (data != null)
				{
					var fileName = MakeFilenameSafe.MakeSafe(UploadedFileName);
					ProcessUploadedFile(data, Path.GetFileName(fileName));
				}
			}

			base.HandleOkButtonClick();
		}

		protected virtual byte[] GetUploadedFileData()
		{
			if (InputFileControl.HasFile)
			{
				using (var fileContent = InputFileControl.FileContent)
				{
					if (fileContent.Length > 0)
					{
						var data = new byte[fileContent.Length];
						var offset = 0;
						var remaining = (int)fileContent.Length;
						while (remaining > 0)
						{
							var read = fileContent.Read(data, offset, remaining);
							remaining -= read;
							offset += read;
						}

						return data;
					}
				}
			}

			return null;
		}

		protected virtual string UploadedFileName => InputFileControl.FileName;

		protected virtual void ProcessUploadedFile(byte[] contents, string fileName)
		{
		}

		protected virtual bool ProceedWithUpload()
		{
			return true;
		}

		protected override string[] OKFunctionArguments
		{
			get { return new string[] { string.Format("'{0}'", ZGuid.NewZGuid().ToString()) }; }
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		protected override bool ShowFooter
		{
			get
			{
				return false;
			}
		}

		protected override bool ShowLoginStatus
		{
			get
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css attribute value should not be translated")]
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			InputFileControl = new InputFile();
			InputFileControl.ID = "InputFile";

			InputFileLabel = new ZTextLabel();
			InputFileLabel.Style[HtmlTextWriterStyle.Width] = GetInputFileLabelWidth();
			InputFileLabel.Style[HtmlTextWriterStyle.TextAlign] = "right";
			InputFileLabel.Style[HtmlTextWriterStyle.PaddingRight] = "5px";
			InputFileLabel.Text = GetInputFileLabelText();

			ProgressBarControl = new ProgressBar();
			ProgressBarControl.ID = "ProgressBar";
			ProgressBarControl.Inline = true;

			if (UploadFileElementsPanel != null)
			{
				UploadFileElementsPanel.Controls.Add(InputFileLabel);
				UploadFileElementsPanel.Controls.Add(InputFileControl);
				UploadFileElementsPanel.Controls.Add(ProgressBarControl);
			}
		}

		protected virtual string GetInputFileLabelWidth()
		{
			return "125px";
		}

		protected virtual string GetInputFileLabelText()
		{
			return Res.GetString("ca7e911b-a62d-44ff-be2b-b8c50cefcb36", "File:");
		}

		protected virtual HtmlGenericControl UploadFileElementsPanel
		{
			get
			{
				if (UploadFilePanel == null)
				{
					if (Form != null)
					{
						UploadFilePanel = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
						Form.Controls.Add(UploadFilePanel);
					}
				}
				return UploadFilePanel;
			}
		}

		protected virtual HtmlGenericControl ButtonsContainerPanel
		{
			get
			{
				if (ButtonsPanel == null)
				{
					if (Form != null)
					{
						ButtonsPanel = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
						Form.Controls.Add(ButtonsPanel);
					}
				}
				return ButtonsContainerPanel;
			}
		}

#if DEBUG
		protected override HtmlForm GetForm(Control parent)
		{
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				return new HtmlForm();
			}
			return base.GetForm(parent);
		}

#endif

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var callerPK = HttpContext.Current.Request?.QueryString[ZIFramePage.CallerPKQuery];
			if (callerPK != null)
			{
				if (ZGuid.TryParse(callerPK, out var indexer))
				{
					DataSourceIndexer = indexer;
				}
			}

			LoadOrCreateDataSource();
			Bind();
		}

		protected override BusinessObject GetNewDataSource()
		{
			return LoadDataSource(DataSourceIndexer) as BusinessObject;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			Title = Res.GetString("827fa8c9-7605-4f0a-a86a-a3091267419a", "Select File to Upload");
		}

		#endregion

		#region Scripts

		protected override void RenderPageSpecificScripts()
		{
			base.RenderPageSpecificScripts();

			RenderFileUploadScript();
		}

		#region FileUploadScript

		const string FileUploadScriptKey = "FileUploadScript";

		void RenderFileUploadScript()
		{
			AddFileUploadScript();
		}

		protected virtual void AddFileUploadScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), FileUploadScriptKey))
			{
				string script = @"<script type=""text/jscript"">        

                                    function CloseDialog(arg)
                                    {
                                        window.returnValue = arg;
                                        window.close();
                                    }

                                 </script>";

				ZClientScript.RegisterClientScriptBlock(GetType(), FileUploadScriptKey, script);
			}
		}

		#endregion

		#endregion

		#region Implementation

		ZTextLabel InputFileLabel;
		HtmlGenericControl UploadFilePanel;
		HtmlGenericControl ButtonsPanel;
		InputFile InputFileControl;
		ProgressBar ProgressBarControl;

		#endregion

		#region Internal Properties
		internal void CreateChildControlsInternal() => CreateChildControls();
		internal void HandleOkButtonClickInternal() => HandleOkButtonClick();

		#endregion
	}
}
