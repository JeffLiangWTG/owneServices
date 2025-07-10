using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.WarehouseIntegration
{
	sealed class BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider : PreviousProceduresFromInventoryForEntryInstructionProvider
	{
		public BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(CusEntryInstruction instruction) : base(instruction)
		{
		}

		protected override IReadOnlyCollection<JobComInvoiceLine> GetApplicableLines()
		{
			return Instruction.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.IsOutOfWarehouseWarehousing).ToList();
		}

		protected override void CreatePreviousProceduresCore()
		{
			var previousDocumentMaster = Instruction.PreviousDocumentMaster;
			var localReferenceNumber = ((CusEntryHeader)Instruction.EntryHeader).LocalReferenceNumber;
			List<(ZString JI_PreviousEntryNumber, ZShort JI_PreviousEntryLineNumber)> bondedWarehouseEntryLineKeyList = new ();
			foreach (var invoiceLine in ApplicableLines)
			{
				UpdateInvoiceLineEntryInfoFromBWHAttribute(invoiceLine);

				var bondedWarehouseEntryLineKey = (invoiceLine.JI_PreviousEntryNumber, invoiceLine.JI_PreviousEntryLineNumber);
				if (!bondedWarehouseEntryLineKeyList.Contains(bondedWarehouseEntryLineKey))
				{
					var totalBondedWhsQuantity = ApplicableLines
						.Where(x => x.JI_PreviousEntryNumber == invoiceLine.JI_PreviousEntryNumber && x.JI_PreviousEntryLineNumber == invoiceLine.JI_PreviousEntryLineNumber)
						.Sum(x => x.JI_BondedWhsQuantity);
					CreatePreviousProcedure(invoiceLine, localReferenceNumber, totalBondedWhsQuantity);
					bondedWarehouseEntryLineKeyList.Add(bondedWarehouseEntryLineKey);
				}
			}
			previousDocumentMaster.CSI_ReferenceNumber2Info.RefreshBinding();
		}

		void UpdateInvoiceLineEntryInfoFromBWHAttribute(JobComInvoiceLine invoiceLine)
		{
			var docketQuery = new ZDBOnlyQuery(typeof(IWhsDocket));
			docketQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, invoiceLine.JI_BondedWHSOrderNumber);
			docketQuery.OrderBy = WhsDocketSchema.WD_ExternalReferenceSplit.Name + OrderByClause.Descending;

			var docket = Factory.LoadTop1<IWhsDocket>(docketQuery);
			if (docket != null)
			{
				var pickLineQuery = new ZDBOnlyQuery(typeof(IWhsDocketLine));

				var docketLineSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WD, docket.PK);
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_LineNo, invoiceLine.JI_BondedWHSOrderLineNumber);

				pickLineQuery.AddSubQuery(WhsPickLineSchema.WZ_WE_TransactionLine, WhsDocketLineSchema.PK, docketLineSubQuery, JoinCondition.And);

				var pickLine = Factory.LoadTop1<IWhsPickLine>(pickLineQuery);

				if (pickLine != null)
				{
					var bwhQuery = new ZDBOnlyQuery(typeof(IWhsBondedWarehouseAttribute));
					bwhQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentID, pickLine.WZ_WE_InventoryLine);
					var bwhAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(bwhQuery);

					if (bwhAttribute != null)
					{
						var entryKey = bwhAttribute.WB_EntryKey;
						var entryLineNo = bwhAttribute.WB_EntryLineNo;

						if (invoiceLine.JI_PreviousEntryNumber != entryKey || invoiceLine.JI_PreviousEntryLineNumber != entryLineNo)
						{
							invoiceLine.JI_PreviousEntryNumber = entryKey;
							invoiceLine.JI_PreviousEntryLineNumber = entryLineNo;
						}
					}
				}
			}
		}
		void CreatePreviousProcedure(JobComInvoiceLine invoiceLine, ZString localReferenceNumber, ZDecimal totalBondedWhsQuantity)
		{
			var previousDocument = GetPreviousDocumentToPopulate(PreviousProcedureList.Codes._ATZL);

			var previousEntryNumber = invoiceLine.JI_PreviousEntryNumber;
			previousDocument.CSI_ReferenceNumber2 = localReferenceNumber;
			previousDocument.CSI_ReferenceNumber = previousEntryNumber;
			previousDocument.CSI_LineNo = invoiceLine.JI_PreviousEntryLineNumber;
			previousDocument.CSI_Tariff = invoiceLine.JI_Tariff;
			previousDocument.Status = previousDocument.CSI_ReferenceNumber.IsValidAtlasReferenceForBondedWarehouse();
			previousDocument.CSI_Quantity2 = totalBondedWhsQuantity;
			previousDocument.CSI_UnitOfQuantity2 = invoiceLine.JI_BondedWhsUnitQty;
		}
	}
}
