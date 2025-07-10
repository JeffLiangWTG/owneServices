using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperParticipation : IDocParticipations
	{
		internal AWBSendChileWrapperParticipation(AsycudaBill bill, ZString participationName)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.participationName = participationName;
			header = this.bill.Header;
			isExport = header.AMA_Nature == ShipmentTypeList.Codes.Export22;
		}
		readonly AsycudaBill bill;
		readonly string participationName;
		readonly AsycudaManifestHeader header;
		readonly ZBool isExport;

		string IDocParticipations.Name => participationName;

		string IDocParticipations.IDType
		{
			get
			{
				var type = ChileOrgCusCodeInfo.OrgCusCodes.RUT;
				var subType = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Cons:
						subType = bill.ABL_ConsigneeRegNoType;
						break;
					case WrappersConstants.ParticipationName.Noti:
						subType = bill.ABL_NotifyPartyRegNoType;
						break;
				}
				if (!subType.IsEmpty)
				{
					type = subType;
				}
				return type;
			}
		}

		string IDocParticipations.IDValue
		{
			get
			{
				var id = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Alm:
						id = bill.GoodsLocation?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ChileOrgCusCodeInfo.OrgCusCodes.RUT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Caer:
						id = header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ChileOrgCusCodeInfo.OrgCusCodes.RUT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Emi:
						id = GlbCompany.CurrentCompany.GC_BusinessRegNo;
						break;
					case WrappersConstants.ParticipationName.Emido:
						id = isExport ? GlbCompany.CurrentCompany.GC_BusinessRegNo : header.ShippingAgent?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ChileOrgCusCodeInfo.OrgCusCodes.RUT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Cnte:
						id = bill.ABL_ShipperRegNo;
						break;
					case WrappersConstants.ParticipationName.Cons:
						id = bill.ABL_ConsigneeRegNo;
						break;
					case WrappersConstants.ParticipationName.Noti:
						id = bill.ABL_NotifyPartyRegNo;
						break;
				}
				return id;
			}
		}

		string IDocParticipations.Names
		{
			get
			{
				var name = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Alm:
						name = bill.GoodsLocation?.Header?.OH_FullName ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Caer:
						name = header.Carrier?.Header?.OH_FullName ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Emi:
						name = GlbCompany.CurrentCompany.GC_Name;
						break;
					case WrappersConstants.ParticipationName.Emido:
						name = isExport ? GlbCompany.CurrentCompany.GC_Name : header.ShippingAgent?.Header?.OH_FullName ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Cnte:
						name = bill.ABL_ShipperName;
						break;
					case WrappersConstants.ParticipationName.Cons:
						name = bill.ABL_ConsigneeName;
						break;
					case WrappersConstants.ParticipationName.Noti:
						name = bill.ABL_NotifyPartyName;
						break;
				}
				return name;
			}
		}
	}
}
