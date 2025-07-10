using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	sealed class InvoiceChargeDocRollUpper : BaseInvoiceDocRollUpper<ZString, MultilingualString>
	{
		public InvoiceChargeDocRollUpper(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docHeader, BusinessObjectFactory factory, DocARInvoiceLineCollection lines, ZString rollUpStyleId, ZString rollUpGroupId, params ZString[] chargeGroups)
			: base(docLineRollUpper, docHeader, factory, lines)
		{
			this.RollUpStyleId = rollUpStyleId;
			ChargeGroupToGroupIdMap = new Dictionary<ZString, ZString>();
			foreach (var chargeGroup in chargeGroups)
			{
				ChargeGroupToGroupIdMap.Add(chargeGroup, rollUpGroupId);
			}
		}

		public InvoiceChargeDocRollUpper(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice header, BusinessObjectFactory factory, DocARInvoiceLineCollection lines, ZString rollUpStyleId, Dictionary<ZString, ZString> chargeGroupToGroupIdMap)
			: base(docLineRollUpper, header, factory, lines)
		{
			this.RollUpStyleId = rollUpStyleId;
			ChargeGroupToGroupIdMap = chargeGroupToGroupIdMap;
		}

		protected override IRolledUpDocLine RollUpGroup(DocARInvoiceLineCollection group, ZString groupId)
			=> RollUpGroupCore
			(
				group,
				groupId
			);

		protected override MultilingualString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId)
		{
			var chargeGroupCode = groupId;
			if (IncludeTaxIdAsPartOfGroupId)
			{
				var pos = chargeGroupCode.IndexOf('%');
				if (pos > 0)
				{
					chargeGroupCode = chargeGroupCode.Substring(0, pos);
				}
			}

			return GetDescriptionByStyleAndGroup(RollUpStyleId, chargeGroupCode);
		}

		internal static MultilingualString GetDescriptionByStyleAndGroup(ZString styleId, ZString groupId)
			=> AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.GetDescription(styleId, groupId);

		bool IncludeTaxIdAsPartOfGroupId => AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule != AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code;

		protected override ZString GetGroupId(IDocLine line)
		{
			var result = ZString.Empty;

			var lineChargeCode = line != null
				? GetChargeCode(line)
				: null;

			if (lineChargeCode != null)
			{
				if (line.PreventGrouping)
				{
					var docARInvoiceLine = (DocARInvoiceLine)line;
					result = docARInvoiceLine.Line.PK.ToString();
				}
				else if (!ChargeGroupToGroupIdMap.TryGetValue(lineChargeCode.AC_ChargeGroup, out result))
				{
					result = ZString.Empty;
				}
			}

			if (result != ZString.Empty && IncludeTaxIdAsPartOfGroupId)
			{
				var docARInvoiceLine = (DocARInvoiceLine)line;
				var lineTaxRate = line != null ? docARInvoiceLine.Line.TaxRate : null;
				result += lineTaxRate != null
					? ("%" + lineTaxRate.PK.ToString())
					: ("%" + ZGuid.Empty.ToString());
			}

			return result;
		}

		protected override void OnBeforeRollUp(BaseDocRollUpGroupList<ZString, DocARInvoiceLineCollection> allGroups)
		{
			if (allGroups.HasGroupThatNeedsRollUp)
			{
				foreach (DocARInvoiceLine line in allGroups.GetLinesThatDontNeedRollUp())
				{
					line.ShowPercentInGSTDisplay = !AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value;
				}
			}

			var originalOrder = allGroups.ToArray();

			allGroups.Sort((DocRollUpGroup<ZString, DocARInvoiceLineCollection> docRollUpGroup1, DocRollUpGroup<ZString, DocARInvoiceLineCollection> docRollUpGroup2) =>
			{
				var result = 0;

				if (docRollUpGroup1.NeedsRollUp != docRollUpGroup2.NeedsRollUp)
				{
					result = docRollUpGroup1.NeedsRollUp ? 1 : -1;
				}
				else if (docRollUpGroup1.NeedsRollUp && docRollUpGroup2.NeedsRollUp && docRollUpGroup1.ID != docRollUpGroup2.ID)
				{
					foreach (var groupId in ChargeGroupToGroupIdMap.Values)
					{
						if (groupId == docRollUpGroup1.ID)
						{
							result = -1;
							break;
						}
						if (groupId == docRollUpGroup2.ID)
						{
							result = 1;
							break;
						}
					}
				}

				if (result == 0 && docRollUpGroup1 != docRollUpGroup2)
				{
					foreach (var group in originalOrder)
					{
						if (group == docRollUpGroup1)
						{
							result = -1;
							break;
						}
						if (group == docRollUpGroup2)
						{
							result = 1;
							break;
						}
					}
				}

				return result;
			});

			base.OnBeforeRollUp(allGroups);
		}

		ZString RollUpStyleId { get; }

		Dictionary<ZString, ZString> ChargeGroupToGroupIdMap { get; }

		AccChargeCode GetChargeCode(IDocLine docLine)
			=> ((DocARInvoiceLine)docLine).Line.ChargeCode;

		public override DocARInvoiceLineCollection GetNewLines(BusinessObjectFactory factory)
			=> DocARInvoiceLineCollection.New(factory);

		protected override BaseDocRollUpGroupList<ZString, DocARInvoiceLineCollection> GetDocRollUpGroupList()
			=> new InvoiceDocRollUpGroupList<ZString>();
	}
}
