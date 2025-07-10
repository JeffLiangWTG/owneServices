using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage, Integration.Customs.IE.ICusAuthorizationUsage
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString AGC_Code
		{
			get => base.AGC_Code;
			set
			{
				var oldValue = AGC_Code;
				base.AGC_Code = value;
				RefreshWarehouseDataIfNeeded(oldValue, AGC_Code);
				DefaultSupportingDocument();
			}
		}

		public override ZString AGC_Number
		{
			get => base.AGC_Number;
			set
			{
				base.AGC_Number = value;
				DefaultSupportingDocument();
			}
		}

		public override ZGuid AGC_OH_Owner
		{
			get => base.AGC_OH_Owner;
			set
			{
				var oldValue = AGC_OH_Owner;
				base.AGC_OH_Owner = value;
				RefreshWarehouseDataIfNeeded(oldValue, AGC_OH_Owner);
			}
		}

		public new CusEntryInstruction Instruction => (CusEntryInstruction)base.Instruction;

		public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;

		protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation()
		{
			EU.Business.CusAuthorizationUsageValidation result = null;
			var parent = Parent;
			if (parent is JobComInvoiceLine invoiceLine)
			{
				if (invoiceLine.IsImport)
				{
					result = new ImportCusAuthorizationUsageValidation(this, invoiceLine);
				}
			}
			else if (parent is CusEntryInstruction instruction)
			{
				if (instruction.IsImport)
				{
					result = new ImportCusAuthorizationUsageValidation(this, instruction);
				}
				else if (instruction.IsExport)
				{
					var strategy = new ExportEntryInstructionCusAuthorizationUsageValidationStrategy(instruction);
					result = new ExportCusAuthorizationUsageValidation(this, strategy);
				}
			}

			return result ?? new CusAuthorizationUsageValidation(this);
		}

		#region CusAuthorisationHeader

		void RefreshWarehouseDataIfNeeded(IZType oldValue, IZType newValue)
		{
			if (suspendWarehouseDataRefreshIndex == 0 && !IsCopying && oldValue != newValue && Instruction is CusEntryInstruction instruction)
			{
				instruction.RefreshWarehouseData();
			}
		}

		internal IDisposable SuspendWarehouseDataRefresh() => new DisposableAction(() => suspendWarehouseDataRefreshIndex++, () => suspendWarehouseDataRefreshIndex--);
		byte suspendWarehouseDataRefreshIndex;

		#endregion

		void DefaultSupportingDocument()
		{
			var referenceNumber = AGC_Number;
			var customsCode = CustomsCode;
			if (!IsCopying && !referenceNumber.IsEmpty && !customsCode.IsEmpty && Parent is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport && !invoiceLine.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				if (!invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == customsCode && x.CSI_ReferenceNumber == referenceNumber))
				{
					var doc = invoiceLine.SupportingDocuments.AddNew();
					doc.CSI_Code = customsCode;
					doc.CSI_ReferenceNumber = referenceNumber;
				}
			}
		}
	}
}
