//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuDocumentConfigValidation
//
//    This class should be used for overriding validation in AutoStmMenuDocumentConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfigValidation : AutoStmMenuDocumentConfigValidation
	{
		public StmMenuDocumentConfigValidation(AutoStmMenuDocumentConfig parent)
			: base(parent)
		{
		}

		protected new StmMenuDocumentConfig Parent
		{
			get { return (StmMenuDocumentConfig)base.Parent; }
		}

		void AddRowError(bool errorCondition, string errorMessage)
		{
			if (errorCondition)
			{
				Parent.AddRowError(errorMessage);
			}
			else
			{
				Parent.RemoveRowError(errorMessage);
			}
		}

		protected override void CheckS3_OverrideDataContext()
		{
			base.CheckS3_OverrideDataContext();
			if (!Parent.OverrideDataContextsList.ContainsCode(Parent.S3_OverrideDataContext))
			{
				Parent.S3_OverrideDataContextInfo.AddError(Res.GetString("e214032a-2c0b-4e26-8896-ff7d2982980a", "Invalid Overriding Data Context."));
			}
		}

		protected override void CheckS3_GC()
		{
			base.CheckS3_GC();

			if (Parent.S3_IsSystem)
			{
				MandatoryValidation.CheckNotEntered(Parent.S3_GCInfo);
			}

			if (!Parent.S3_GCInfo.HasErrors())
			{
				foreach (StmMenuDocumentConfig docConfig in Parent.GetRelatedDocConfigs())
				{
					if (!Parent.S3_IsTemplate && !docConfig.S3_IsTemplate)
					{
						if (docConfig.PK != Parent.PK
							&& docConfig.S3_SI == Parent.S3_SI
							&& docConfig.S3_IsSystem == Parent.S3_IsSystem
							&& docConfig.S3_OH == Parent.S3_OH
							&& (Parent.S3_IsSystem || docConfig.S3_GC == Parent.S3_GC))
						{
							Parent.S3_GCInfo.AddError(
								Res.GetString("c958373e-2654-4d57-a6ba-c6274eef9184", "There is already another document configuration with the same combination of values for {0}, {1} and {2}.",
								Parent.S3_IsSystemInfo.HumanReadableName, Parent.S3_GCInfo.HumanReadableName, Parent.S3_OHInfo.HumanReadableName));
						}
					}
				}
			}
		}

		protected override void CheckS3_Description()
		{
			base.CheckS3_Description();
			if (Parent.S3_IsSystem && string.IsNullOrEmpty(Parent.S3_Description))
			{
				Parent.S3_DescriptionInfo.AddError(Res.GetString("CBFDB014-519E-4A7F-9050-C5E7B6767BBE", "Description cannot be empty when it is a system document"));
				MandatoryValidation.CheckEntered(Parent.S3_DescriptionInfo);
			}
		}

		protected override void CheckS3_IsSystem()
		{
			base.CheckS3_IsSystem();
			if (Parent.S3_IsSystem)
			{
				StmMenuTemplatePivot menuTemplatePivot = Parent.MenuTemplatePivot;
				if ((menuTemplatePivot != null) && (!menuTemplatePivot.SI_IsSystemDefined))
				{
					Parent.S3_IsSystemInfo.AddError(Res.GetString("00d78861-eae9-4869-a850-46ab5509be6e", "This document configuration cannot be marked as system-defined because the containing document is not system-defined."));
				}
			}
			ValidateS3_OH();
			ValidateS3_IsTemplate();
		}

		protected override void CheckS3_IsTemplate()
		{
			base.CheckS3_IsTemplate();
			if (Parent.S3_IsTemplate && !Parent.S3_IsSystem)
			{
				Parent.S3_IsTemplateInfo.AddError(Res.GetString("caae9679-148b-48f0-8933-0c20ab49e1c2", "This document configuration cannot be marked as template because it is not system-defined."));
			}
			ValidateS3_OH();

			if (!Parent.S3_IsTemplate && Parent.S3_IsSystem)
			{
				if (Parent.GetRelatedDocConfigs().Any(item => item.S3_IsSystem && !item.S3_IsTemplate))
				{
					Parent.S3_IsTemplateInfo.AddError(Res.GetString("448DFB2D-22C1-44CD-AE85-F6AB1AEDB2F5", "You cannot have more than one system non-template document configuration."));
				}
			}
		}

		protected override void CheckS3_OH()
		{
			base.CheckS3_OH();
			if (Parent.S3_IsSystem)
			{
				MandatoryValidation.CheckNotEntered(Parent.S3_OHInfo);
			}
			ValidateS3_GC();
		}

		protected override void CheckS3_PageStyle()
		{
			base.CheckS3_PageStyle();
			MandatoryValidation.CheckEntered(Parent.S3_PageStyleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S3_PageStyleInfo);
		}

		protected override void CheckS3_ExcludedFromDocPack()
		{
			base.CheckS3_ExcludedFromDocPack();

			var relatedDocConfigs = Parent.GetRelatedDocConfigs();
			if (relatedDocConfigs.All(x => x.S3_ExcludedFromDocPack) && Parent.S3_ExcludedFromDocPack)
			{
				Parent.S3_ExcludedFromDocPackInfo.AddError(Res.GetString("d19f644f-0b54-4373-8d28-2a0b1b44778a", "Can not exclude system defined document configuration from document pack without any other document configurations added in the document pack"));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			AddRowError(Parent.ConfigItems.Count == 0, Res.GetString("cdc2a476-c0ee-496f-98ab-6a2fbeb94009", "Please add at least one section."));
		}
	}
}
