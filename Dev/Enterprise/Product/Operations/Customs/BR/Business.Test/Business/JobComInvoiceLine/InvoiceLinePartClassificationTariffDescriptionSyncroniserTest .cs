using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		#region TariffCode

		protected override ZString TariffCode
		{
			get
			{
				return "0000000000";
			}
		}

		protected override ZString TariffCode2
		{
			get
			{
				return "0000.00.00.01K";
			}
		}

		#endregion

		#region TariffDescription

		protected override ZString TariffDescription => "";

		protected override ZString TariffDescription2 => "";

		#endregion

		#region DeclarationTypeForTest

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(JobDeclaration); }
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			if (declaration.JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge)
			{
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			}
		}

		#endregion
	}
}
