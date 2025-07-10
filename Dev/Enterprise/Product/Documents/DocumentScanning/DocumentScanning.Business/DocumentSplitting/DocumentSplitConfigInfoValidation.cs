using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitConfigInfoValidation : ZValidation
	{
		public DocumentSplitConfigInfoValidation(DocumentSplitConfigInfo parent)
				: base(parent)
		{
			this.parent = parent;
			zValidationInternals = this;
			parentListInternals = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateDocumentName();
			ValidateDocumentType();
			ValidateStartPage();
			ValidateEndPage();
		}

		#endregion

		#region DocumentName

		public void ValidateDocumentName()
		{
			zValidationInternals.Validate(Parent.DocumentNameInfo, CheckDocumentName);
		}

		void CheckDocumentName()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentNameInfo);

			if (Parent.DocumentNameInfo.HasErrors())
			{
				return;
			}

			if (Parent.DocumentName.Length > AutoStorageDocs.Schema.SC_FileNameMaxLength)
			{
				Parent.DocumentNameInfo.AddError(Res.GetString("FB5EC589-98CD-4A72-ADBF-291FFDFB4C8D", "This filename is longer than {0} characters.", AutoStorageDocs.Schema.SC_FileNameMaxLength));
			}
			else if (!IsValidFileName(Parent.DocumentName))
			{
				Parent.DocumentNameInfo.AddError(Res.GetString("DACB7247-C07E-4BAF-BECD-604D89D23433", "Filename contains Invalid character(s). The name should be a Windows compatible file name."));
			}
			else if (CargoWise.IO.MakeFilenameSafe.IsWindowsReservedFileName(Parent.DocumentName))
			{
				Parent.DocumentNameInfo.AddError(Res.GetString("4E38D64A-F5C3-4C54-A665-954018F53C45", "Filename is Microsoft MS-DOS reserved. Please choose another filename."));
			}
		}

		bool IsValidFileName(ZString fileName)
		{
			return !fileName.ContainsAnyChar(new string(Path.GetInvalidFileNameChars()));
		}

		#endregion

		#region DocumentType

		public void ValidateDocumentType()
		{
			zValidationInternals.Validate(Parent.DocumentTypeInfo, GetDocumentTypeValidationInvoker());
		}

		RunValidationInvoker GetDocumentTypeValidationInvoker()
		{
			return delegate
			{
				CheckDocumentTypeIsWesternEuropean();
				CheckDocumentType();
			};
		}

		protected void CheckDocumentTypeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.DocumentTypeInfo);
		}

		void CheckDocumentType()
		{
			var checkpoint = Env.Security.GetDocumentTypeUploadCheckPoint(Parent.DocumentType);
			if (!checkpoint.IsAllowed)
			{
				Parent.DocumentTypeInfo.AddError(checkpoint.ErrorMessageForNotAllowed);
			}

			if (!Parent.DocumentTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.DocumentTypeInfo, Parent.DocType_List);
				MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			}
		}

		#endregion

		#region StartPage

		public void ValidateStartPage()
		{
			zValidationInternals.Validate(Parent.StartPageInfo, CheckStartPage);
		}

		void CheckStartPage()
		{
			if (Parent.StartPage <= 0 || Parent.StartPage > Parent.SourceDocumentPageCount)
			{
				Parent.StartPageInfo.AddError(Res.GetString("f325331a-69c1-4029-9cee-3570895b89b3", "Page number should be inside the page range (1 to {0}) of the source document", Parent.SourceDocumentPageCount));
			}
			if (Parent.StartPage > Parent.EndPage)
			{
				Parent.StartPageInfo.AddError(Res.GetString("74eC5ace-c7f9-41c0-9c79-eeb9b3437239", "Start page number should be smaller than end page number."));
			}
		}

		#endregion

		#region EndPage

		public void ValidateEndPage()
		{
			zValidationInternals.Validate(Parent.EndPageInfo, CheckEndPage);
		}

		void CheckEndPage()
		{
			if (Parent.EndPage <= 0 || Parent.EndPage > Parent.SourceDocumentPageCount)
			{
				Parent.EndPageInfo.AddError(Res.GetString("f325331a-69c1-4029-9cee-3570895b89b3", "Page number should be inside the page range (1 to {0}) of the source document", Parent.SourceDocumentPageCount));
			}
			if (Parent.EndPage < Parent.StartPage)
			{
				Parent.EndPageInfo.AddError(Res.GetString("c6c528d6-3693-4373-849a-6daa0f5acd0c", "End page number should be bigger than start page number."));
			}
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get
			{
				return typeof(DocumentSplitConfigInfo);
			}
		}

		public DocumentSplitConfigInfo Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly DocumentSplitConfigInfo parent;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
