using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	public class DCImportFlatFileConverter : WCBDataConverter
	{
		public DCImportFlatFileConverter(INotifications notification, BusinessObjectFactory factory, bool isMercedes)
			: base(notification, factory)
		{
			this.IsMercedes = isMercedes;
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
		{
			if (!IsHeaderAndFooterValid(rows))
			{
				throw new WCBException(WCBException.WCBExceptionType.InvalidFileFormat);
			}

			DCUniqueInvoiceBuilder.UniqueInvoices = new Dictionary<ZString, DCUniqueInvoice>();
			ZString invoiceKey = ZString.Empty;
			foreach (FlatFileDataRow row in rows)
			{
				DecInvoiceHeaderDataRow headerRow = row as DecInvoiceHeaderDataRow;
				DecInvoiceLineDataRow lineRow = row as DecInvoiceLineDataRow;
				if (headerRow != null)
				{
					invoiceKey = GetInvoiceKey(headerRow);
					DCUniqueInvoiceBuilder.AddNewHeaderOnly(invoiceKey, headerRow);
				}
				else if (lineRow != null && !invoiceKey.IsEmpty)
				{
					DCUniqueInvoiceBuilder.AddLineOnly(invoiceKey, lineRow);
				}
			}

			Xsd.ConsolAndShipmentCollection xmlDeclarations = valueObject as Xsd.ConsolAndShipmentCollection;
			Xsd.ConsolAndShipment xmlDeclaration = null;
			foreach (KeyValuePair<ZString, DCUniqueInvoice> uniqueInvoice in DCUniqueInvoiceBuilder.UniqueInvoices)
			{
				var topLineRow = uniqueInvoice.Value.InvoiceLineCollection[0] ?? throw new WCBException(WCBException.WCBExceptionType.NoInvoiceLinesForHeader, uniqueInvoice.Value.InvoiceHeader.ToString());

				xmlDeclaration = BuildInvoiceHeader(uniqueInvoice.Value.InvoiceHeader, uniqueInvoice.Value.InvoiceLineCollection[0]);

				foreach (DecInvoiceLineDataRow invoiceLine in uniqueInvoice.Value.InvoiceLineCollection)
				{
					BuildInvoiceLine(xmlDeclaration, invoiceLine);
				}

				xmlDeclarations.Add(xmlDeclaration);
			}
		}

		static ZString GetInvoiceKey(DecInvoiceHeaderDataRow headerRow)
		{
			if (headerRow.RegionalAllocation.IsEmpty)
			{
				throw new WCBException(WCBException.WCBExceptionType.NoPortCodeFound, headerRow.ToString());
			}
			return string.Format("{0}\t{1}", headerRow.OceanBill, headerRow.RegionalAllocation);
		}

		static bool IsHeaderAndFooterValid(FlatFileDataRowCollection rows)
		{
			bool result = false;
			if (rows != null && rows.Count >= 2)
			{
				string pattern = @"[*]{3}[B,E]OF:(FGA|Daimler)Chrysler<(\s{4})?[1,2][0-9]{3}[0,1][0-9][0-3][0-9]><[0-2][0-9][0-5][0-9][0-5][0-9]>[*]{3}";
				string header = rows[0].GetField(0);
				string footer = rows[rows.Count - 1].GetField(0);

				result = (Regex.IsMatch(header, pattern) && Regex.IsMatch(footer, pattern));
			}
			return result;
		}

		Xsd.ConsolAndShipment BuildInvoiceHeader(DecInvoiceHeaderDataRow headerRow, DecInvoiceLineDataRow topLineRow)
		{
			Xsd.ConsolAndShipment result = new Xsd.ConsolAndShipment();
			if (headerRow.VesselName.IsEmpty)
			{
				result.Consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
				Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
				flight.FlightNoJourneyNoTruckRegNo = headerRow.Voyage;
				result.Consol.ConsolDetail.Item = flight;
			}
			else
			{
				result.Consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
				Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
				sailing.VesselName = headerRow.VesselName;
				sailing.VoyageNo = headerRow.Voyage;
				result.Consol.ConsolDetail.Item = sailing;
			}

			Xsd.Shipment xsdShipment = result.Consol.Shipments.AddNew();
			Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Masterbill = headerRow.OceanBill;
			xsdShipment.ShipmentDetails.Consignee = GetOrganisation(IsMercedes ? WCBDataRegistry.Instance.MercedesImporter : WCBDataRegistry.Instance.ChryslerImporter);
			xsdShipment.ShipmentDetails.Consignor = GetOrganisation(IsMercedes ? WCBDataRegistry.Instance.MercedesSupplier : WCBDataRegistry.Instance.ChryslerSupplier);
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = headerRow.RegionalAllocation;
			xsdShipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.IMP;

			Xsd.InvoiceHeader xsdInvHead = result.Consol.Shipments[0].Invoices.AddNew();
			xsdInvHead.InvoiceNumber = topLineRow.InvoiceNumber;
			return result;
		}

		void BuildInvoiceLine(Xsd.ConsolAndShipment xmlDec, DecInvoiceLineDataRow row)
		{
			Xsd.InvoiceHeader xsdInvHead = xmlDec.Consol.Shipments[0].Invoices[0];
			xsdInvHead.InvoiceAmount.Value += Xsd.FinancialValue.FromAmountAndCurrency(row.FOBAmount, GlbBranch.CurrentBranch.Country.LocalCurrency).Value;

			Xsd.InvoiceLine xsdInvLine = xsdInvHead.InvoiceLines.AddNew();
			xsdInvLine.PartAttrib1 = row.CommissionNo;
			xsdInvLine.ProductNumber = row.Model;
			SetUsePartAttrib1Flags(xsdInvLine.ProductNumber);
			xsdInvLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(row.FOBAmount, GlbBranch.CurrentBranch.Country.LocalCurrency);
			xsdInvLine.OrderNumber = row.OrderNumber;
			xsdInvLine.InvoiceQty.Value = 1M;
			xsdInvLine.InvoiceQty.DimensionType = "NO";
		}

		Xsd.Organisation GetOrganisation(ZGuid orgPK)
		{
			Xsd.Organisation result = new Xsd.Organisation();

			OrgHeader org = (OrgHeader)Factory.Load(typeof(OrgHeader), orgPK);
			if (org != null)
			{
				result.EDICode = org.OH_Code;
			}

			return result;
		}

		void SetUsePartAttrib1Flags(string productNumber)
		{
			OrgSupplierPart part = (OrgSupplierPart)Factory.LoadTop1(typeof(OrgSupplierPart), new ZQuery(OrgSupplierPartSchema.OP_PartNum, productNumber));
			if (part != null)
			{
				foreach (OrgPartRelation relation in part.RelatedOrganisations)
				{
					if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
							relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					{
						relation.OU_UsePartAttrib1 = true;
					}
				}
			}
		}

		readonly bool IsMercedes;
	}
}
