using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ECUFlatFileConverter : FlatFileConverter
	{
		public ECUFlatFileConverter(INotifications notification, BusinessObjectFactory factory, ZString fileID)
			: base(notification, factory)
		{
			this.FileID = fileID;
		}

		#region MapExport

		FlatFileDataRowCollection Rows
		{
			get
			{
				if (fRows == null)
				{
					fRows = new FlatFileDataRowCollection();
				}

				return fRows;
			}
		}

		FlatFileDataRowCollection fRows;

		internal ZString EmailAddress
		{
			get { return fEmailAddress; }
		}

		ZString fEmailAddress = ZString.Empty;

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			Xsd.Consol consol = (Xsd.Consol)valueObject;

			try
			{
				if (IsValidToExport(consol.ConsolDetail))
				{
					Xsd.Organisation recvAgent = consol.ConsolDetail.ReceivingAgent;

					if (recvAgent.OrganisationDetails.EDITransmissionDetails.Type == Xsd.OrganisationDetailEDITransmissionDetailsType.EMA)
					{
						fEmailAddress = recvAgent.OrganisationDetails.EDITransmissionDetails.Address;
					}

					SetHeaderDetails(recvAgent, consol.ConsolDetail.SendingAgent);
					SetConsolDetails(consol);
				}

				if (Rows.Count == 0)
				{
					Notification.Notify(new ErrorNotification(ErrorType.Error, "Not Valid for Export"));
					fHasErrors = true;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Caught Exception in DataExport of ECUDataConverter", e);
			}

			return Rows;
		}
		internal FlatFileDataRowCollection InternalMapExport(IValueObject valueObject) => MapExport(valueObject);

		#endregion

		#region HasErrors

		public bool HasErrors
		{
			get { return fHasErrors; }
		}

		bool fHasErrors;

		#endregion

		#region IsValidToExport

		bool IsValidToExport(Xsd.ConsolConsolDetail consolDetail)
		{
			bool recvAgentIsSpecified = consolDetail.ReceivingAgent.IsSpecified;

			if (!recvAgentIsSpecified)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, "Receiving Agent is not Specified"));
			}

			return recvAgentIsSpecified;
		}

		#endregion

		#region GetECUAgentcode

		ZString GetECUAgentCode(Xsd.RegistrationNumberCollection registrationNumbers)
		{
			ZString result = "";

			if (registrationNumbers.IsSpecified)
			{
				ZString countryCode = Env.CurrentCompany.Country.Code;
				foreach (Xsd.RegistrationNumber number in registrationNumbers)
				{
					if (number.NumberType == Xsd.RegistrationNumberTypes.GTN && number.CountryOfRegistration == countryCode)
					{
						result = number.Number;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region SetHeaderDetails

		void SetHeaderDetails(Xsd.Organisation recvAgent, Xsd.Organisation sendingAgent)
		{
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GVER, Constants.VersionNumber));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GSTS, Constants.Production));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GDOC, Constants.Manifest));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GREF, FileID.PadLeft(8, '0')));

			ZString senderInfo = Constants.SubHeader.Code + GetECUAgentCode(sendingAgent.OrganisationDetails.RegistrationNumbers) +
									Constants.SubHeader.Mail + Env.CurrentUser.EmailAddress;

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GSND, senderInfo));

			ZString receiverInfo = Constants.SubHeader.Code + GetECUAgentCode(recvAgent.OrganisationDetails.RegistrationNumbers) + Constants.SubHeader.Mail + EmailAddress;
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GRCV, receiverInfo));

			ZDateTime now = ZDateTime.Now;
			ZString dateTimeInfo = Constants.SubHeader.Date + now.ToString("yyyy.MM.dd")
									+ Constants.SubHeader.Time + now.ToString("HH.mm.ss");

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.GDAT, dateTimeInfo));
		}

		#endregion

		#region SetConsolDetails

		void SetConsolDetails(Xsd.Consol consol)
		{
			if (consol.ConsolDetail.Item.IsSpecified && consol.ConsolDetail.Item is Xsd.SailingWithVesselVoyage)
			{
				ZString vessel = ((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VesselName;
				Rows.Add(new ECUFlatFileDataRow(Constants.Header.VVES, vessel));

				ZString voyage = ((Xsd.SailingWithVesselVoyage)consol.ConsolDetail.Item).VoyageNo;
				Rows.Add(new ECUFlatFileDataRow(Constants.Header.VOYN, voyage));
			}

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VCAR, consol.ConsolDetail.Carrier.OrganisationDetails.Name));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VLAG, consol.ConsolDetail.SendingAgent.OrganisationDetails.Name));

			SetPortsDetails(consol.ConsolDetail);
			SetContainerDetails(consol);

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.DONE, FileID.PadLeft(8, '0')));
		}

		#endregion

		#region SetContainerDetails

		void SetContainerDetails(Xsd.Consol consol)
		{
			ZDecimal containersVolume = 0;
			ZDecimal containersWeight = 0;
			ZInt containersPackageCount = 0;
			ZString packageType = "";

			ZString paymentType = GetPaymentType(consol.ConsolDetail.PaymentType.ToString());

			foreach (Xsd.Container container in consol.ConsolDetail.Containers)
			{
				ZString size = container.ContainerType.ContainerCode.SubstringSafe(0, 2);
				ZString code = container.ContainerType.ContainerCode.SubstringSafe(2, 2);

				Rows.Add(new ECUFlatFileDataRow(Constants.Header.CNTR, container.ContainerNumber));
				if (size.IsNumbersOnlyOrEmpty)
				{
					ZString containerInfo = Constants.SubHeader.Numb + size + Constants.SubHeader.Code + code;
					Rows.Add(new ECUFlatFileDataRow(Constants.Header.CTYP, containerInfo));
				}

				Rows.Add(new ECUFlatFileDataRow(Constants.Header.CSLN, container.Seal));

				Xsd.ShipmentCollection shipments = consol.GetShipmentsForContainer(container.ContainerNumber);

				if (shipments.Count != 0)
				{
					ContainerPackageInfo containerPackageInfo = SetAndReturnContainerShipmentDetails(shipments, container.ContainerNumber, paymentType);

					containersVolume += containerPackageInfo.Volume;
					containersWeight += containerPackageInfo.Weight;
					containersPackageCount += containerPackageInfo.PackageCount;
					packageType = containerPackageInfo.PackageType;
				}
			}

			SetContainerTotalWeightAndVolume(containersVolume, containersWeight, containersPackageCount, packageType, true);
		}

		#endregion

		#region SetPortsDetails

		void SetPortsDetails(Xsd.ConsolConsolDetail consolDetail)
		{
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VPOL, consolDetail.PortOfLoading.Port.Value.SubstringSafe(2, 3)));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VPOD, consolDetail.PortOfDischarge.Port.Value.SubstringSafe(2, 3)));

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VETD, consolDetail.PortOfLoading.EstimatedDateTime.ToString("yyyy.MM.dd")));
			Rows.Add(new ECUFlatFileDataRow(Constants.Header.VETA, consolDetail.PortOfDischarge.EstimatedDateTime.ToString("yyyy.MM.dd")));
		}

		#endregion

		#region SetAndReturnContainerShipmentDetails

		ContainerPackageInfo SetAndReturnContainerShipmentDetails(Xsd.ShipmentCollection shipments, ZString containerNumber, ZString paymentType)
		{
			ZDecimal containerVolume = 0;
			ZDecimal containerWeight = 0;
			ZInt containerPackageCount = 0;
			ZString packageType = "";

			foreach (Xsd.Shipment shipment in shipments)
			{
				Xsd.UNLOCO origin = shipment.ShipmentDetails.PortOfOrigin.Port;
				Xsd.UNLOCO dest = shipment.ShipmentDetails.PortofDestination.Port;

				Rows.Add(new ECUFlatFileDataRow(Constants.Header.BHBL, shipment.Housebill));

				if (shipment.ShipmentDetails.Consignor.IsSpecified)
				{
					SetOrganisationDetails(shipment.ShipmentDetails.Consignor, Constants.Header.BSHP, false);
				}

				if (shipment.ShipmentDetails.Consignee.IsSpecified)
				{
					SetOrganisationDetails(shipment.ShipmentDetails.Consignee, Constants.Header.BCON, true);
				}

				if (shipment.ShipmentDetails.NotifyParty.IsSpecified)
				{
					if (shipment.ShipmentDetails.Consignee.EDICode != shipment.ShipmentDetails.NotifyParty.Organisation.EDICode)
					{
						SetOrganisationDetails(shipment.ShipmentDetails.NotifyParty.Organisation, Constants.Header.BNOT, false);
					}
					else
					{
						Rows.Add(new ECUFlatFileDataRow(Constants.Header.BNOT, Constants.SubHeader.Name + "SAME AS CONSIGNEE"));
					}
				}

				ZString originInfo = origin.City;
				Rows.Add(new ECUFlatFileDataRow(Constants.Header.BBPL, originInfo));

				ZString destInfo = dest.City + "," + dest.Country;
				Rows.Add(new ECUFlatFileDataRow(Constants.Header.BBPD, destInfo));
				Rows.Add(new ECUFlatFileDataRow(Constants.Header.BDES, destInfo));

				SetBOLDetails(shipment.ShipmentDetails, paymentType);

				ContainerPackageInfo containerPackageInfo = SetAndReturnPackagesDetails(shipment, containerNumber);

				containerVolume += containerPackageInfo.Volume;
				containerWeight += containerPackageInfo.Weight;
				containerPackageCount += containerPackageInfo.PackageCount;
				packageType = containerPackageInfo.PackageType;
			}

			SetContainerTotalWeightAndVolume(containerVolume, containerWeight, containerPackageCount, packageType, false);

			return new ContainerPackageInfo(containerPackageCount, containerVolume, containerWeight, packageType);
		}

		#endregion

		#region SetBOLDetails

		void SetBOLDetails(Xsd.ShipmentShipmentDetails shipmentDetails, ZString paymentType)
		{
			ZString freightAmtInfo = Constants.SubHeader.Code + paymentType + Constants.SubHeader.Numb
										+ shipmentDetails.FreightRate.Value.ToString(2)
										+ Constants.SubHeader.Curr
										+ shipmentDetails.FreightRate.CurrencyCode
										+ Constants.SubHeader.Unit + "BL";

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BFRT, freightAmtInfo));

			ZString numberOfBL = Constants.SubHeader.Numb + shipmentDetails.NoOriginalBills
								 + Constants.SubHeader.Full + NumToWords(shipmentDetails.NoOriginalBills);

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BNBL, numberOfBL));
		}

		#endregion

		#region SetContainerTotalWeightAndVolume

		void SetContainerTotalWeightAndVolume(ZDecimal containerVolume, ZDecimal containerWeight, int containerPackageCount, ZString packageType, bool forAllContainers)
		{
			ZString packageCountHeader = (forAllContainers) ? Constants.Header.TPKG : Constants.Header.CPKG;
			ZString weightHeader = (forAllContainers) ? Constants.Header.TWGT : Constants.Header.CWGT;
			ZString volumeHeader = (forAllContainers) ? Constants.Header.TMEA : Constants.Header.CMEA;

			Rows.Add(new ECUFlatFileDataRow(packageCountHeader, Constants.SubHeader.Numb + containerPackageCount + Constants.SubHeader.Unit + packageType));
			Rows.Add(new ECUFlatFileDataRow(weightHeader, Constants.SubHeader.Numb + containerWeight + Constants.SubHeader.Unit + Constants.WeightUQ));
			Rows.Add(new ECUFlatFileDataRow(volumeHeader, Constants.SubHeader.Numb + containerVolume + Constants.SubHeader.Unit + Constants.VolUQ));
		}

		#endregion

		#region GetPaymentType

		ZString GetPaymentType(ZString payment)
		{
			ZString result = "";

			if (payment == Core.Constants.PaymentType.Collect)
			{
				result = "CC";
			}
			else if (payment == Core.Constants.PaymentType.Prepaid)
			{
				result = "PP";
			}

			return result;
		}

		#endregion

		#region NumToWords

		protected static readonly string[] PrimitiveNumbers = { "", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };

		ZString NumToWords(ZString noOfBillsStr)
		{
			int noOfBills = int.Parse(noOfBillsStr);

			ZString result = "";

			if (noOfBills < 20)
			{
				result = PrimitiveNumbers[noOfBills];
			}

			return result;
		}

		#endregion

		#region PackageUnitList

		RefPackTypeCollection PackageUnitList
		{
			get
			{
				if (fPackageUnitList == null)
				{
					fPackageUnitList = new RefPackTypeCollection(Factory);
				}

				return fPackageUnitList;
			}
		}

		RefPackTypeCollection fPackageUnitList;

		#endregion

		#region SetAndReturnPackagesDetails

		ContainerPackageInfo SetAndReturnPackagesDetails(Xsd.Shipment shipment, ZString containerNumber)
		{
			ZDecimal volume = shipment.ShipmentDetails.Volume.Value;
			ZDecimal weight = shipment.ShipmentDetails.Weight.Value;
			int packageCount = shipment.ShipmentDetails.TotalOuterPacksQty.Value.ToZInt();

			RefPackTypeCollection list = new RefPackTypeCollection(Factory);

			ZString? packageTypeDescription = PackageUnitList.GetDescriptionFromCode(shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType);
			ZString packageType = (packageTypeDescription.HasValue) ? new ZString(packageTypeDescription).ToUpper() : ZString.Empty;

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BSET, Constants.SubHeader.Strt + "01"));

			SetMarksAndNumbersOrGoodsDesc(shipment.ShipmentDetails.MarksAndNumbers, 61, false);

			ZString numOfPacks = Constants.SubHeader.Numb + packageCount.ToString()
									+ Constants.SubHeader.Unit + packageType;

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BPKG, numOfPacks));

			ZString weightInfo = Constants.SubHeader.Numb + weight.ToString()
								+ Constants.SubHeader.Unit + shipment.ShipmentDetails.Weight.DimensionType;

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BWGT, weightInfo));

			ZString volumeInfo = Constants.SubHeader.Numb + volume.ToString()
								+ Constants.SubHeader.Unit + shipment.ShipmentDetails.Volume.DimensionType;

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BMEA, volumeInfo));

			SetMarksAndNumbersOrGoodsDesc(shipment.ShipmentDetails.GoodsDescription, 61, true);

			Rows.Add(new ECUFlatFileDataRow(Constants.Header.BSET, Constants.SubHeader.Stop + "01"));

			return new ContainerPackageInfo(packageCount, volume, weight, packageType);
		}

		#endregion

		#region SetMarksAndNumbersOrGoodsDesc

		void SetMarksAndNumbersOrGoodsDesc(ZString desc, int maxLength, bool isGoodsDesc)
		{
			const int MaxLineNo = 15;
			ZString description = desc;
			ZString line = ZString.Empty;

			ZString header = (isGoodsDesc) ? Constants.Header.BDSC : Constants.Header.BMKN;

			for (int lineNo = 1; description.Length > 0 && lineNo <= MaxLineNo; lineNo++)
			{
				int lineFeedIndex = description.IndexOf("\r\n");
				ZString lnStr = lineNo.ToString().PadLeft(2, '0');

				if (lineFeedIndex > -1 && lineFeedIndex < maxLength)
				{
					line = description.SubstringSafe(0, lineFeedIndex);
				}
				else
				{
					line = description.SubstringSafe(0, maxLength);
				}

				description = description.SubstringSafe(line.Length);

				if (description.StartsWith("\r\n"))
				{
					description = description.SubstringSafe(2);
				}

				Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Ln + lnStr + ">" + line));
			}
		}

		#endregion

		#region SetOrganisationDetails

		void SetOrganisationDetails(Xsd.Organisation organisation, ZString header, bool setFaxAndEmail)
		{
			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.GetMainAddress();
			Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Name + organisation.OrganisationDetails.Name));

			if (!address.AddressLine1.IsEmpty)
			{
				Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Adr1 + address.AddressLine1));
			}

			if (!address.AddressLine2.IsEmpty)
			{
				Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Adr2 + address.AddressLine2));
			}

			if (!(address.CityOrSuburb.IsEmpty && address.StateOrProvince.IsEmpty && address.PostCode.IsEmpty))
			{
				Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Adr3 + address.CityOrSuburb + " " +
											address.StateOrProvince + " " + address.PostCode + " "));
			}
			if (setFaxAndEmail)
			{
				ZString fax = address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax);

				if (!fax.IsEmpty)
				{
					Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Faxn + fax));
				}

				ZString email = address.Email;

				if (!email.IsEmpty)
				{
					Rows.Add(new ECUFlatFileDataRow(header, Constants.SubHeader.Mail + email));
				}
			}
		}

		#endregion

		readonly ZString FileID;
	}
}
