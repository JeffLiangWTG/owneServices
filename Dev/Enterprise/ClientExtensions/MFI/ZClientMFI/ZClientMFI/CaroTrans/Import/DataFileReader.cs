using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Client.MFI.CaroTrans.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Data
{
	public class CarotransManifestDataFileReader
	{
		public CarotransManifestDataFileReader(string data)
		{
			Lines = data.Trim().Split('\n');
			foreach (string lineFromFile in Lines)
			{
				var csvLine = new OCsvLine(lineFromFile);
				DetermineDataRowType(csvLine.FieldValues);
			}
		}

		readonly string[] Lines;

		public Xsd.ConsolCollection ProcessImportManifest()
		{
			Xsd.ConsolCollection consols = new Xsd.ConsolCollection();

			foreach (string[] oceanBill in OceanBills)
			{
				Xsd.Consol consol = consols.AddNew();
				Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
				identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
				identifier.ConsolIdentifierTypeSpecified = true;
				identifier.Value = oceanBill[Constants.ContainerRecord.ContainerId];

				consol.ConsolDetail.ConsolType = Xsd.ConsolType.Agent;
				consol.ConsolDetail.ConsolTypeSpecified = true;
				consol.ConsolDetail.ContainerMode = DeterminePackingContainerMode(oceanBill[Constants.ContainerRecord.NumberOfBills], oceanBill[Constants.ContainerRecord.ContainerNumber], "CONSOL");
				consol.ConsolDetail.ContainerModeSpecified = true;
				consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
				consol.ConsolDetail.TransportModeSpecified = true;
				consol.ConsolDetail.PortOfLoading = ProcessPortDetail(oceanBill[Constants.ContainerRecord.PortOfLoading], oceanBill[Constants.ContainerRecord.SailDate]);
				consol.ConsolDetail.PortOfDischarge = ProcessPortDetail(oceanBill[Constants.ContainerRecord.PortOfDischarge], oceanBill[Constants.ContainerRecord.ArrivalDate]);
				consol.ConsolDetail.Item = ProcessVesselDetail(oceanBill[Constants.ContainerRecord.Voyage], oceanBill[Constants.ContainerRecord.Vessel]);
				consol.ConsolDetail.SendingAgent = ProcessOrganisation(oceanBill[Constants.ContainerRecord.PortOfLoading], "");
				consol.ConsolDetail.AgentReference = oceanBill[Constants.ContainerRecord.ContainerId];
				consol.ConsolDetail.Containers = ProcessContainers(oceanBill[Constants.ContainerRecord.ContainerId]);

				consol.Shipments = ProcessShipments(oceanBill, consol.ConsolDetail.ContainerMode);
			}

			return consols;
		}

		#region ProcessShipments

		protected Xsd.ShipmentCollection ProcessShipments(string[] oceanBill, Xsd.ContainerMode containerMode)
		{
			Xsd.ShipmentCollection result = new Xsd.ShipmentCollection();

			foreach (string[] container in Containers)
			{
				if (container[Constants.ContainerRecord.ContainerId] == oceanBill[Constants.ContainerRecord.ContainerId])
				{
					foreach (string[] bill in Bills)
					{
						if (bill[Constants.BillRecord.ContainerNumber] == container[Constants.ContainerRecord.ContainerNumber])
						{
							Xsd.Shipment shipment = result.AddNew();

							Xsd.ShipmentIdentifier shipIdentifier = shipment.ShipmentIdentifier.AddNew();
							shipIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
							shipIdentifier.Value = bill[Constants.BillRecord.BOL];

							shipment.ShipmentDetailsSpecified = true;

							shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
							string inco = (bill[Constants.BillRecord.Terms].ToUpper() == "P") ? "CIF" : "FOB";
							shipment.ShipmentDetails.Incoterm = inco;

							shipment.ShipmentDetails.Consignee = ProcessOrganisationDetails(
								bill[Constants.BillRecord.MFIAgentCode],
								bill[Constants.BillRecord.CTIAgentCode],
								bill[Constants.BillRecord.ConsigneeName],
								"",
								bill[Constants.BillRecord.ConsigneeAdd1],
								bill[Constants.BillRecord.ConsigneeAdd2] + bill[Constants.BillRecord.ConsigneeAdd3],
								bill[Constants.BillRecord.ConsigneeCity],
								bill[Constants.BillRecord.ConsigneeState],
								bill[Constants.BillRecord.ConsigneeZip],
								bill[Constants.BillRecord.ConsigneePhone],
								bill[Constants.BillRecord.ConsigneeFax]);

							shipment.ShipmentDetails.Consignor = ProcessOrganisationDetails("",
								bill[Constants.BillRecord.ShipperCode],
								bill[Constants.BillRecord.ShipperName],
								"",
								bill[Constants.BillRecord.ShipperAdd1],
								bill[Constants.BillRecord.ShipperAdd2] + bill[Constants.BillRecord.ShipperAdd3],
																bill[Constants.BillRecord.ShipperCity],
								bill[Constants.BillRecord.ShipperState],
								bill[Constants.BillRecord.ShipperZip],
								bill[Constants.BillRecord.ShipperPhone],
								bill[Constants.BillRecord.ShipperFax]);

							bool isNotifyPartySameAsConsignee = (((ZString)bill[Constants.BillRecord.NotifyName].Trim().ToUpper()).Left(4) == "SAME");
							if (!isNotifyPartySameAsConsignee)
							{
								shipment.ShipmentDetails.NotifyParty = ProcessContactDetails(bill[Constants.BillRecord.NotifyName],
									bill[Constants.BillRecord.ConsigneeCode],
									bill[Constants.BillRecord.ConsigneeName],
									"", bill[Constants.BillRecord.ConsigneeAdd1],
									bill[Constants.BillRecord.ConsigneeAdd2] + bill[Constants.BillRecord.ConsigneeAdd3],
									bill[Constants.BillRecord.ConsigneeCity],
									bill[Constants.BillRecord.ConsigneeState],
									bill[Constants.BillRecord.ConsigneeZip],
									bill[Constants.BillRecord.ConsigneePhone],
									bill[Constants.BillRecord.ConsigneeFax]);
							}

							shipment.ShipmentDetails.PackingMode = DeterminePackingContainerMode(oceanBill[Constants.ContainerRecord.NumberOfBills],
								oceanBill[Constants.ContainerRecord.ContainerNumber], "JOB");
							shipment.ShipmentDetails.PackingModeSpecified = true;

							ProcessBillBodys(shipment, bill);
							shipment.ShipmentDetails.Custom.IsSpecified = false;

							if (shipment.ShipmentDetails.PackingMode == Xsd.ContainerMode.LCL)
							{
								shipment.ShipmentDetails.PortOfOrigin = ProcessPortDetail(shipment.Housebill.Left(3),
									oceanBill[Constants.ContainerRecord.SailDate]);
							}
							else if (shipment.ShipmentDetails.PackingMode == Xsd.ContainerMode.FCL)
							{
								shipment.ShipmentDetails.PortOfOrigin = ProcessPortDetail(oceanBill[Constants.ContainerRecord.PortOfLoading],
									oceanBill[Constants.ContainerRecord.SailDate]);
							}

							shipment.ShipmentDetails.PortofDestination = ProcessPortDetail(shipment.Housebill.SubstringSafe(3, 3),
								oceanBill[Constants.ContainerRecord.ArrivalDate]);
						}
					}
				}
			}

			return result;
		}

		void ProcessBillBodys(Xsd.Shipment shipment, string[] bill)
		{
			ZString marks = ZString.Empty;
			ZString description = ZString.Empty;

			foreach (string[] billBody in BillBodys)
			{
				if (billBody[Constants.BillBodyRecord.BOL] == bill[Constants.BillRecord.BOL])
				{
					if (billBody[Constants.BillBodyRecord.SequenceNumber] == "1")
					{
						int totalOuterPacksQty = 0;
						int.TryParse(billBody[Constants.BillBodyRecord.Packages], out totalOuterPacksQty);
						shipment.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)totalOuterPacksQty, Core.Constants.PkgUnit.Unit);

						ZDecimal volume = 0;
						ZDecimal.TryParse(billBody[Constants.BillBodyRecord.CBM], out volume);
						shipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(volume, Core.Constants.Volume.CubicMetres);

						ZDecimal weight = 0;
						ZDecimal.TryParse(billBody[Constants.BillBodyRecord.Weight], out weight);
						shipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(weight, Core.Constants.Weight.Kilograms);

						shipment.ShipmentDetails.TotalInnerPacksQty.IsSpecified = true;
						shipment.ShipmentDetails.ChargeableWeight.IsSpecified = true;

						Xsd.Package package = shipment.ShipmentDetails.Packages.AddNew();
						package.NumberOfPacks = (uint)totalOuterPacksQty;
						package.PackType = Core.Constants.PkgUnit.Unit;
						package.Volume = shipment.ShipmentDetails.Volume;
						package.Weight = shipment.ShipmentDetails.Weight;
						package.HazardousGoods.IsSpecified = false;
						package.ContainerNumber = bill[Constants.BillRecord.ContainerNumber];
						package.Width.IsSpecified = true;
						package.Length.IsSpecified = true;
						package.Height.IsSpecified = true;
					}
					marks += billBody[Constants.BillBodyRecord.Marks];
					description += billBody[Constants.BillBodyRecord.Description];
				}
			}

			shipment.ShipmentDetails.MarksAndNumbers = marks;
			shipment.ShipmentDetails.GoodsDescription = description;

			bool isExpressBillRelease = description.ToLower().Contains("express");
			if (isExpressBillRelease)
			{
				shipment.ShipmentDetails.ReleaseType = Xsd.ReleaseType.EBL;
				shipment.ShipmentDetails.ReleaseTypeSpecified = true;
			}
		}

		#endregion

		#region SubProcesses

		protected Xsd.SailingWithVesselVoyage ProcessVesselDetail(string voyageNo, string vesselName)
		{
			Xsd.SailingWithVesselVoyage result = new Xsd.SailingWithVesselVoyage();

			result.VesselName = vesselName;
			result.VoyageNo = voyageNo;

			return result;
		}

		protected Xsd.ContainerCollection ProcessContainers(string containerId)
		{
			Xsd.ContainerCollection result = new Xsd.ContainerCollection();

			foreach (string[] containerRecord in Containers)
			{
				if (containerRecord[Constants.ContainerRecord.ContainerId] == containerId)
				{
					Xsd.Container container = result.AddNew();
					container.ContainerNumber = containerRecord[Constants.ContainerRecord.ContainerNumber];
					container.ContainerType = new Xsd.ContainerType();

					string containerSize = ((ZString)containerRecord[Constants.ContainerRecord.ContainerSize]).SubstringSafe(0, 2);
					container.ContainerType.ISOCode = containerSize == "40" ? "42G0" : "22G0";
					container.PackingMode = DeterminePackingContainerMode(containerRecord[Constants.ContainerRecord.NumberOfBills], containerRecord[Constants.ContainerRecord.ContainerNumber], "CONSOL");
					container.Seal = "1234";
				}
			}

			return result;
		}

		protected Xsd.ContainerMode DeterminePackingContainerMode(string numberOfBills, string containerNumber, string type)
		{
			Xsd.ContainerMode result = new Xsd.ContainerMode();
			numberOfBills = numberOfBills.Trim();
			containerNumber = containerNumber.Trim();
			string consignee = "";
			bool moreThanOneConsignee = false;

			if (numberOfBills != "1")
			{
				foreach (string[] bill in Bills)
				{
					if (containerNumber == bill[Constants.BillRecord.ContainerNumber].Trim())
					{
						if (string.IsNullOrEmpty(consignee))
						{
							consignee = bill[Constants.BillRecord.ConsigneeCode] + bill[Constants.BillRecord.ConsigneeName];
						}
						else if (consignee != bill[Constants.BillRecord.ConsigneeCode] + bill[Constants.BillRecord.ConsigneeName])
						{
							moreThanOneConsignee = true;
						}
					}
				}
			}

			if (numberOfBills != "1" && moreThanOneConsignee)
			{
				result = type == "CONSOL" ? Xsd.ContainerMode.GRP : Xsd.ContainerMode.LCL;
			}
			else if (numberOfBills != "1" && !moreThanOneConsignee)
			{
				result = type == "CONSOL" ? Xsd.ContainerMode.BCN : Xsd.ContainerMode.LCL;
			}
			else
			{
				result = Xsd.ContainerMode.FCL;
			}

			return result;
		}

		protected Xsd.Movement ProcessPortDetail(string portCode, string edtDate)
		{
			Xsd.Movement result = new Xsd.Movement();
			result.Port = Xsd.UNLOCO.FromPortCode(Factory, portCode);
			result.EstimatedDateTime = new ZDateTime(ProcessDate(edtDate), DateTimeKind.Local);
			return result;
		}

		protected Xsd.Organisation ProcessOrganisation(string ownerCode, string ediCode)
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OwnerCode = ownerCode;
			result.EDICode = ediCode;
			return result;
		}

		protected Xsd.Organisation ProcessOrganisationDetails(string ediCode, string ownerCode, string name, string location, string add1,
										string add2, string city, string state, string postCode, string phone, string fax)
		{
			Xsd.Organisation result = ProcessOrganisation(ownerCode, ediCode);
			result.OrganisationDetails = new Xsd.OrganisationDetail();
			result.OrganisationDetails.Name = name;
			result.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress address = new Xsd.OrgAddress();
			address.AddressLine1 = add1;
			address.AddressLine2 = add2;
			address.CityOrSuburb = city;
			address.StateOrProvince = state;
			address.PostCode = postCode;

			result.OrganisationDetails.Addresses.Add(address);
			address.TelephoneNumbers = new Xsd.TelephoneNumberCollection();

			if (!string.IsNullOrEmpty(phone))
			{
				Xsd.TelephoneNumber businessTelephone = new Xsd.TelephoneNumber();
				businessTelephone.NumberType = Xsd.TelephoneNumberNumberType.Business;
				businessTelephone.Value = phone;

				address.TelephoneNumbers.Add(businessTelephone);
			}

			if (!string.IsNullOrEmpty(fax))
			{
				Xsd.TelephoneNumber faxTelephone = new Xsd.TelephoneNumber();
				faxTelephone.NumberType = Xsd.TelephoneNumberNumberType.Fax;
				faxTelephone.Value = fax;

				address.TelephoneNumbers.Add(faxTelephone);
			}

			return result;
		}

		protected Xsd.ContactReference ProcessContactDetails(string contactName, string ownerCode, string name, string location, string add1,
			string add2, string city, string state, string postCode, string phone, string fax)
		{
			Xsd.ContactReference result = new Xsd.ContactReference();
			result.Organisation = ProcessOrganisationDetails("", ownerCode, name, location, add1, add2, city, state, postCode, phone, fax);

			result.Organisation.OrganisationDetails.Contacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact contact = result.Organisation.OrganisationDetails.Contacts.AddNew();
			contact.Name = contactName;
			contact.Sequence = 1;
			result.ContactSequenceRef = 1;

			return result;
		}

		protected DateTime ProcessDate(ZString initialDate)
		{
			int month = Int16.Parse(initialDate.Substring(4, 2));
			int day = Int16.Parse(initialDate.Right(2));
			int year = Int16.Parse(initialDate.Left(4));

			return new DateTime(year, month, day);
		}

		#endregion

		#region Collections

		protected ArrayList Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new ArrayList();
				}
				return fContainers;
			}
		}
		ArrayList fContainers;

		protected ArrayList Bills
		{
			get
			{
				if (fBills == null)
				{
					fBills = new ArrayList();
				}
				return fBills;
			}
		}
		ArrayList fBills;

		protected ArrayList BillBodys
		{
			get
			{
				if (fBillBodys == null)
				{
					fBillBodys = new ArrayList();
				}
				return fBillBodys;
			}
		}
		ArrayList fBillBodys;

		protected ArrayList OceanBills
		{
			get
			{
				if (fOceanBills == null)
				{
					fOceanBills = new ArrayList();
				}
				return fOceanBills;
			}
		}
		ArrayList fOceanBills;

		#endregion

		#region Implementation

		protected void DetermineDataRowType(string[] dataRow)
		{
			if (dataRow.Length > 1)
			{
				switch (dataRow[Constants.ContainerRecord.RecordType])
				{
					case Constants.RecordTypes.ContainerRec:
						if (dataRow.Length == 22)
						{
							DetermineUniqueOBL(dataRow);
							Containers.Add(dataRow);
						}
						break;

					case Constants.RecordTypes.BillRec:

						if (dataRow.Length == 44)
						{
							Bills.Add(dataRow);
						}
						break;

					case Constants.RecordTypes.BillBodyRec:
						if (dataRow.Length == 10)
						{
							BillBodys.Add(dataRow);
						}
						break;
				}
			}
		}

		protected void DetermineUniqueOBL(string[] container)
		{
			bool isFound = false;
			foreach (string[] oceanBill in OceanBills)
			{
				if (oceanBill[Constants.ContainerRecord.ContainerId] == container[Constants.ContainerRecord.ContainerId])
				{
					isFound = true;
					break;
				}
			}

			if (!isFound)
			{
				OceanBills.Add(container);
			}
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		#endregion
	}
}
