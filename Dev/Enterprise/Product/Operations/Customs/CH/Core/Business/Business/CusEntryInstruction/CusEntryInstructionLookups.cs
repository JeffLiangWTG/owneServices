using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
		: base(cusEntryInstruction)
	{
	}

	new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

	ZDateTime DateOfValuation => Parent?.DateOfValuation ?? ZDateTime.Today;

	public override CodeDescriptionPairList StyleList
	{
		get
		{
			if (Parent?.JobDeclaration?.IsExportOrExportDeclarationActivation ?? false)
			{
				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.InputControl, DateOfValuation);
			}
			else
			{
				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, DateOfValuation);
			}
		}
	}

	public override CodeDescriptionPairList EntrySubStyleList => CommonLookups.DeclarationTimeCodeList(Parent);

	public override OrgHeaderCollection Owners => new ConsigneeCollection(Factory);

	public ICodeDescriptionPairList ProcedureCodeList
	{
		get
		{
			return Factory.GetCachedValue("CH.CusEntryInstructionLookups.ProcedureCodeList",
				() => RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, Core.Constants.CountryCodes.Switzerland, JobMessageTypeList.Codes.Export, Parent?.JobDeclaration?.DateOfValuation ?? ZDateTime.Today));
		}
	}

	public CodeDescriptionPairList WarehouseTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.WarehouseType, ZDateTime.Today);

	public CodeDescriptionPairList DeclarationReasonList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.DeclarationReason, DateOfValuation);

	public CodeDescriptionPairList TransportChargesMethodOfPaymentList => Factory.GetCachedValue<TransportChargesModeOfPayment>();

	public CodeDescriptionPairList NextProcedureList => CommonLookups.NextProcedureList(Parent);
}
