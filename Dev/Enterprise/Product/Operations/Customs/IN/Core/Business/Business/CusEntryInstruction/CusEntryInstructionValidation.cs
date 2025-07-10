using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public class CusEntryInstructionValidation : AutoINCusEntryInstructionValidation
{
	public CusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateRBIWaiverNumber();
		ValidateRBIWaiverDate();
		ValidateTotalInvoices();
		using (((ISingleElementListInternal)Parent).SuspendListChanged())
		{
			ValidateShippingBillNumber();
			ValidateShippingBillDate();
		}
	}

	protected override void ValidateStyleList()
	{
		if (Parent.IsExport)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_StyleInfo);
		}
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();
		if (Parent.IsExport && !Parent.CEI_SubStyle_ReadOnly)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_SubStyleInfo);
		}
	}

	protected override void CheckCEI_NumberOfPackages()
	{
		base.CheckCEI_NumberOfPackages();
		var parent = Parent;
		if (parent.IsExport)
		{
			MandatoryValidation.WarnIfNotEntered(parent.CEI_NumberOfPackagesInfo);
		}
	}

	public void ValidateRBIWaiverNumber()
	{
		ValidateCalculatedProperty(Parent.RBIWaiverNumberInfo);
	}

	protected void CheckRBIWaiverNumber()
	{
		var parent = Parent;
		if (parent.IsExport)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.RBIWaiverNumberInfo, parent.CEI_SubStyleInfo);
		}
	}

	public void ValidateRBIWaiverDate()
	{
		ValidateCalculatedProperty(Parent.RBIWaiverDateInfo);
	}

	protected void CheckRBIWaiverDate()
	{
		var parent = Parent;
		if (parent.IsExport)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.RBIWaiverDateInfo, parent.CEI_SubStyleInfo);
		}
	}

	protected override void CheckCEI_TotalContainer()
	{
		base.CheckCEI_TotalContainer();
		var parent = Parent;
		if (parent.IsExport && parent.CEI_TotalContainer > 99 && (parent.JobDeclaration?.IsContainerised ?? false))
		{
			parent.CEI_TotalContainerInfo.AddMessageError(Res.GetString("BA81844D-F812-4AE5-92DE-8E4B4FFBD009", "Only 99 containers are allowed per Entry Instructions."));
		}
	}

	protected void ValidateTotalInvoices()
	{
		var parent = Parent;
		int GetTotalInvoices() => parent.InvoiceLines.Select(l => l.JI_JZ).Where(x => x.IsValid).Distinct().Count();
		if (parent.IsExport && GetTotalInvoices() > 99)
		{
			parent.AddRowMessageError(Res.GetString("885C2922-6C80-4B83-BDB8-3CE539E377DC", "Only 99 Invoices are supported under one Entry Instructions/Shipping Bill."));
		}
	}

	public void ValidateShippingBillNumber()
	{
		ValidateCalculatedProperty(Parent.ShippingBillNumberInfo);
	}

	protected void CheckShippingBillNumber()
	{
		if (!Parent.IsExport || !Parent.ShippingBillNumberOverride)
		{
			return;
		}

		var info = Parent.ShippingBillNumberInfo;
		if (!Parent.ShippingBillNumber.IsNumbersOnlyOrEmpty)
		{
			info.AddError(Res.GetString("144D31CC-C35B-4151-8BBF-6D814444C693", "Shipping Bill No. must be numeric"));
		}

		if (!info.Notifications.HasErrors() && !Parent.ShippingBillNumber.IsEmpty && Parent.ShippingBillDate.IsValid
			&& GetDeclarationWithSameSBNumberInSameFinancialYear() is { } declarationWithSameSBNumber)
		{
			var otherJobNum = declarationWithSameSBNumber.JobNumber ?? ZString.Empty;
			info.AddError(Res.GetString("55F116E7-4EC2-42CF-A61C-3CF05C05E02B", "Shipping Bill No. is already present in {0}", otherJobNum));
		}
	}

	BaseJobDeclaration GetDeclarationWithSameSBNumberInSameFinancialYear()
	{
		var financialYearRange = Utils.GetIndianFinancialYearRange(Parent.ShippingBillDate.Date);

		bool HasDuplicatedSBNumber(CusEntryInstruction x) => x.PK != Parent.PK && x.ShippingBillNumber == Parent.ShippingBillNumber && x.ShippingBillDate.Date >= financialYearRange.startDate && x.ShippingBillDate.Date <= financialYearRange.endDate;

		var hasDuplicatedSBNumberInParentJob = Parent.JobDeclaration?.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(HasDuplicatedSBNumber) ?? false;
		if (hasDuplicatedSBNumberInParentJob)
		{
			return Parent.JobDeclaration;
		}

		var entryNumberParentQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		{
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, Parent.ShippingBillNumber);
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, financialYearRange.startDate);
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, financialYearRange.endDate);
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Indian.ShippingBill);
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			entryNumberParentQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, Parent.PK);
		}

		var instructionQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey);
		instructionQuery.AddSubQuery(CusEntryInstructionSchema.PK, entryNumberParentQuery, JoinCondition.And);
		var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
		declarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, instructionQuery, JoinCondition.And);
		return Parent.Factory.LoadTop1<JobDeclaration>(declarationQuery);
	}

	public void ValidateShippingBillDate()
	{
		ValidateCalculatedProperty(Parent.ShippingBillDateInfo);
	}

	protected void CheckShippingBillDate()
	{
		if (!Parent.IsExport || !Parent.ShippingBillNumberOverride)
		{
			return;
		}

		var info = Parent.ShippingBillDateInfo;
		TypeValidation.CheckValidSmallDateTime(info);

		if (!info.Notifications.HasErrors() && Parent.ShippingBillDate.IsInTheFuture())
		{
			info.AddError(Res.GetString("5515A86F-0535-4D20-BBA6-AEDAE6165D52", "Shipping Bill Date cannot be a future date."));
		}

		ValidateShippingBillNumber();
	}

	protected override void CheckCEI_WeightUQ()
	{
		base.CheckCEI_WeightUQ();
		var parent = Parent;
		if (parent.IsExport)
		{
			var netWeight = parent.NetWeight;
			var grossWeight = parent.GrossWeight;
			var weightUQInfo = Parent.CEI_WeightUQInfo;
			const decimal maxAllowedGrossWeight = 9999999999.999m;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_WeightUQInfo);

			if (grossWeight > maxAllowedGrossWeight)
			{
				weightUQInfo.AddMessageError(Res.GetString("536F71F5-692B-44D3-A5CA-D3D90B193470", "Gross Weight is greater than Max Value supported {0}.", maxAllowedGrossWeight));
			}

			if (grossWeight < netWeight)
			{
				weightUQInfo.AddMessageError(Res.GetString("52BD9C9F-A565-4B9B-8E6D-9DE6D65CED1C", "Gross Weight is less than Net Weight."));
			}
		}
	}
}
