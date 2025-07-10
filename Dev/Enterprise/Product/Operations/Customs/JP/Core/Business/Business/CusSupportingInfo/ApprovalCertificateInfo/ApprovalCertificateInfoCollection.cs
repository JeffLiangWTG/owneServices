using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;

namespace Enterprise.Customs.JP.Business
{
	public class ApprovalCertificateInfoCollection : CusSupportingInfoCollection<ApprovalCertificateInfo>
	{
		public ApprovalCertificateInfoCollection(CusEntryInstruction entryInstruction)
			: base(entryInstruction, CusSupportingInfoTypeList.Codes.ApprovalCertificate)
		{
			declaration = entryInstruction.JobDeclaration;
			if (declaration != null)
			{
				RefereshCountValidation();
				declaration.JE_MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		readonly BaseJobDeclaration declaration;
		CusEntryInstruction entryInstruction => Master as CusEntryInstruction;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (entryInstruction != null && !entryInstruction.IsValidationSuspended && entryInstruction.Validation is CusEntryInstructionValidation validation)
			{
				validation.ValidateApprovalCertificateInfos();
				validation.ValidateCEI_Style();
			}
		}

		void RefereshCountValidation()
		{
			var maxCount = GetMaxCount();

			var message =
				maxCount == -1
					? string.Empty
					: Res.GetString("76394453-B34B-4FA9-9140-849A8FE1A50F", "The row count has exceeded the maximum limit. The maximum allowed number of rows is {0}.", maxCount);

			this.EnableMaxCountValidationWithMessageError(maxCount, false, message);
		}

		void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefereshCountValidation();
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < GetMaxCount();

		int GetMaxCount()
		{
			var result = -1;

			switch (declaration?.JE_MessageType ?? string.Empty)
			{
				case JobMessageTypeList.Codes.Import:
					result = MaxCountForImport;
					break;
				case JobMessageTypeList.Codes.Export:
					result = MaxCountForExport;
					break;
			}

			return result;
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(ApprovalCertificateInfo);

		public static int MaxCountForImport => 10;

		public static int MaxCountForExport => 15;
	}
}
