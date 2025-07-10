using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration;

public partial class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected string ValidateJE_DeclarantTypeAndOperators(string typeOperator)
	{
		string message = string.Empty;
		if (Parent.JE_DeclarantType == ESRepresentationTypeList.Codes._1Auto)
		{
			message = CheckJE_DeclarantTypeIsSelfDispatch(typeOperator);
		}
		else if (Parent.JE_DeclarantType == ESRepresentationTypeList.Codes._2Direct || Parent.JE_DeclarantType == ESRepresentationTypeList.Codes._5IndirectATC)
		{
			message = CheckJE_DeclarantTypeIsDirect(typeOperator);
		}
		else if (Parent.JE_DeclarantType == ESRepresentationTypeList.Codes._3Indirect || Parent.JE_DeclarantType == ESRepresentationTypeList.Codes._4DirectATC)
		{
			message = CheckJE_DeclarantTypeIsIndirect(typeOperator);
		}
		return message;
	}

	protected string CheckJE_DeclarantTypeIsSelfDispatch(string typeOperator)
	{
		string message = string.Empty;
		if (Parent.Representative != null && listDecTypeAndDecRepresent.Contains(typeOperator))
		{
			message = Res.GetString("2325C3A0-BF26-42DC-9836-C8DE7D55EF9D", "For Rep. Type 1 (Self Dispatch) no Representative should be declared.");
		}
		else if (Parent.DeclarantOrgAddress != null && listDecTypeAndDecAddress.Contains(typeOperator) && Parent.DeclarantOrgAddress.OA_OH.IsValid && Parent.JE_OH_Supplier.IsValid && Parent.DeclarantOrgAddress.OA_OH != Parent.JE_OH_Supplier)
		{
			message = Res.GetString("7459BE58-A184-40CA-870E-C4774AB94315", "For Rep. Type 1 (Self Dispatch), Declarant and Exporter must coincide.");
		}
		return message;
	}

	protected string CheckJE_DeclarantTypeIsDirect(string typeOperator)
	{
		string message = string.Empty;
		if ((Parent.Representative == null || !Parent.Representative.OA_OH.IsValid) && (Parent.DeclarantOrgAddress == null || !Parent.DeclarantOrgAddress.OA_OH.IsValid) && typeOperator == MessageForJE_DeclarantType)
		{
			message = Res.GetString("3C8C1346-AEBF-47CD-B5AB-8E265611FCF3", "For Rep. Type 2 or 5 (Direct) a Representative/Declarant must be declared.");
		}
		else if (Parent.Representative != null && Parent.DeclarantOrgAddress != null && Parent.DeclarantOrgAddress.OA_OH.IsValid && Parent.JE_OH_Supplier.IsValid && Parent.DeclarantOrgAddress.OA_OH != Parent.JE_OH_Supplier && listDecTypeAndDecAddress.Contains(typeOperator))
		{
				message = Res.GetString("E525423C-BC24-4204-B7FD-1D7E528C40CC", "For Rep. Type 2 or 5 (Direct), Declarant and Exporter must coincide.");
		}
		return message;
	}

	protected string CheckJE_DeclarantTypeIsIndirect(string typeOperator)
	{
		string message = string.Empty;
		if ((Parent.Representative == null || !Parent.Representative.OA_OH.IsValid) && (Parent.DeclarantOrgAddress == null || !Parent.DeclarantOrgAddress.OA_OH.IsValid) && typeOperator == MessageForJE_DeclarantType)
		{
			message = Res.GetString("833C785C-4588-4CEE-AD43-C2BE254B62F6", "For Rep. Type 3 or 4 (Indirect) a Representative/Declarant must be declared.");
		}
		else if (Parent.Representative != null && Parent.DeclarantOrgAddress != null && Parent.Representative.OA_OH.IsValid && Parent.DeclarantOrgAddress.OA_OH.IsValid && Parent.Representative.OA_OH != Parent.DeclarantOrgAddress.OA_OH && listDecTypeAndDecAddress.Contains(typeOperator))
		{
			message = Res.GetString("EB510505-9E4B-47F9-B6DD-26D51C965248", "For Rep. Type 3 or 4 (Indirect), Representative and Declarant must coincide or one of them must be empty.");
		}
		return message;
	}

	protected override void CheckJE_DeclarantType()
	{
		base.CheckJE_DeclarantType();
		var message = ValidateJE_DeclarantTypeAndOperators(MessageForJE_DeclarantType);
		if (!message.IsNullOrEmpty())
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(message);
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		var message = ValidateJE_DeclarantTypeAndOperators(MessageForJE_OA_DeclarantAddress);
		if (!message.IsNullOrEmpty())
		{
			Parent.JE_OA_DeclarantAddressInfo.AddMessageError(message);
		}
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();
		var message = ValidateJE_DeclarantTypeAndOperators(MessageForJE_OA_Representative);
		if (!message.IsNullOrEmpty())
		{
			Parent.JE_OA_RepresentativeInfo.AddMessageError(message);
		}
	}

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();
		var customsOfficeCode = Parent.GetExitCustomsOffice();

		var goodsLocationCode = Parent.JE_LocationOfGoods.Left(8);
		if (Parent.JE_TransportModeInland.IsEmpty && customsOfficeCode != goodsLocationCode)
		{
			Parent.JE_TransportModeInlandInfo.AddWarning(Res.GetString("846088EF-6FBE-40AE-B16E-6449B98BB61F", "[26] Inland M.O.T. is mandatory for indirect exits."));
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		if (Parent.HasAnyDiffT2CAndT2lAndEXSEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}

	protected override void CheckJE_OH_ShippingLine()
	{
		base.CheckJE_OH_ShippingLine();
		if (Parent.HasAnyEXSEntry && Parent.JE_OH_ShippingLine.IsEmpty)
		{
			Parent.JE_OH_ShippingLineInfo.AddMessageError(Res.GetString("0B0B1291-6416-40BB-B777-68E1FBC0A436", "You have not entered a Carrier"));
		}
	}

	protected override void CheckJE_OH_Supplier()
	{
		base.CheckJE_OH_Supplier();
		if (Parent.HasAnyDiffT2CEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
		}
	}

	protected override void CheckJE_RL_NKFinalDestination()
	{
		base.CheckJE_RL_NKFinalDestination();
		if (Parent.HasAnyDiffT2CEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);
		}
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();
		if (Parent.HasAnyDiffT2CEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
		}
	}

	protected override void CheckJE_RL_NKOrigin()
	{
		base.CheckJE_RL_NKOrigin();
		if (Parent.HasAnyDiffT2CEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKOriginInfo);
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		if (Parent.HasAnyDiffT2CAndT2lEntry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}
	}

	protected override ZBool RequireJE_ShipmentIncoTermPlaceMandatory => !Parent.IsUCC6 && base.RequireJE_ShipmentIncoTermPlaceMandatory;

	protected override void CheckJE_ShipmentIncoTerm()
	{
		base.CheckJE_ShipmentIncoTerm();

		if (Parent.IsUCC6 && Parent.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry && Parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_IncoTerm.IsEmpty))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ShipmentIncoTermInfo);
		}
	}

	readonly List<string> listDecTypeAndDecAddress = new List<string> { "T", "D" };
	readonly List<string> listDecTypeAndDecRepresent = new List<string> { "T", "R" };
	const string MessageForJE_DeclarantType = "T";
	const string MessageForJE_OA_DeclarantAddress = "D";
	const string MessageForJE_OA_Representative = "R";
}
