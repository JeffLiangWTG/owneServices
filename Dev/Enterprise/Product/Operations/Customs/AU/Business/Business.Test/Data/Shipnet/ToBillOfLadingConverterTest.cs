using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ToBillOfLadingConverterTest : TestCaseWithFactory
	{
		public void TestConversionEndToEnd()
		{
			using (var reader = new StreamReader(embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("ImportIMM2.TXT"))))
			{
				var notification = new NotificationBuffer();
				var converter = new ToBillOfLadingConverter(notification, Factory);

				var bols = new Xsd.AgencyBillsOfLading();

				converter.ImportFlatFile(bols, new ShipnetFlatFileFormat(), reader);

				AssertEquals(3, bols.AgencyBillOfLading.Count);
				{
					Xsd.AgencyBillOfLading bol = bols.AgencyBillOfLading[0];
					AssertEquals("DLCBNE01P5BB01A", bol.BillNumber);
					AssertEquals("Default Release Type", Xsd.AgencyReleaseType.OBR, bol.ReleaseType);

					AssertEquals(new ZDateTime(2005, 03, 17, 0, 0, 0), bol.Sailing.ETA);
					AssertEquals(new ZDateTime(2005, 03, 15, 0, 0, 0), bol.Sailing.ETD);

					AssertEquals("9231145", bol.Sailing.LloydsNo);
					AssertEquals("Cape Darnley", bol.Sailing.VesselName);
					AssertEquals("01P5S", bol.Sailing.VoyageNo);

					AssertEquals("", bol.BookingReference);
					AssertEquals("", bol.ShippersReference);
					AssertEquals("", bol.InterimReceipt);

					AssertEquals("318 PACKAGES STEEL STRUCTURES 752,015KGS 4,449.04M3 39 PKGS SCRATCHED 9 PKGS ONE STEEL STRAP BAND BROKEN ( NO.266,128,245,160,138,105,133,299 AND 285) 2 PKGS TWO STEEL STRAP BANDS BROKEN ( NO.241 AND 130) 3 PKGS MARKED BY HANDLING/REPAIRING EQUIPMENT", bol.Description);
					AssertEquals("KOGAN CREEK POWER STATION 1 X 750MW COAL FIRED BOILER AND AUXILIARY EQUIPMENT C/NO.:XKC-EMT-02 PO NO.:E-MT-0001 DIMENSIONS:(L)X(W)X(H) MASS: KGS TO: KOGAN CREEK POWER STATION COUNTRY OF ORIGIN : CHINA", bol.MarksAndNumbers);

					AssertEquals("PPD", bol.INCOTerm);
					AssertEquals("", bol.ChargesApply);

					//AssertEquals(false, bol.CustomsEntryNumber.IsSpecified);

					AssertEquals(318, bol.PackageSummary.NumberOfPacks);
					AssertEquals(Core.Constants.PkgUnit.Package, bol.PackageSummary.PackType);
					AssertEquals("KG", bol.PackageSummary.Weight.DimensionType);
					AssertEquals(new ZDecimal(752015.0), bol.PackageSummary.Weight.Value);
					AssertEquals("M3", bol.PackageSummary.Volume.DimensionType);
					AssertEquals(new ZDecimal(4449.04), bol.PackageSummary.Volume.Value);

					AssertEquals(ZDateTime.Empty, bol.IssueDate);
					AssertEquals("", bol.ShippedOnBoard.Code);
					AssertEquals(ZDateTime.Empty, bol.ShippedOnBoard.Date);
					AssertEquals((byte)0, bol.OriginalBills);
					AssertEquals((byte)0, bol.CopyBills);

					AssertEquals(Xsd.ContainerMode.BBK, bol.CargoType);

					AssertEquals("CNDLC", bol.Load.Value);
					AssertEquals("CNDLC", bol.Origin.Value);
					AssertEquals("AUBNE", bol.Discharge.Value);
					AssertEquals("AUBNE", bol.Destination.Value);

					AssertEquals(0, bol.Containers.Count);
					AssertEquals(1, bol.Packages.Count);

					AssertEquals(318, bol.Packages[0].NumberOfPacks);
					AssertEquals(Core.Constants.PkgUnit.Package, bol.Packages[0].PackType);
					AssertEquals("KG", bol.Packages[0].Weight.DimensionType);
					AssertEquals(new ZDecimal(752015.0), bol.Packages[0].Weight.Value);
					AssertEquals("M3", bol.Packages[0].Volume.DimensionType);
					AssertEquals(new ZDecimal(4449.04), bol.Packages[0].Volume.Value);
					//AssertEquals(false, bol.Packages[0].HazardousGoods.IsSpecified); TODO: fix
					AssertEquals("Kogan Creek Second Shipment", bol.Packages[0].GoodsDescription);
					AssertEquals("", bol.Packages[0].MarksAndNumbers);

					AssertEquals(5, bol.Addresses.DocAddress.Count);
					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CRD);
						AssertEquals("CHINAEMTYO", address.AddressCode);
						AssertEquals("China Emoto Construction Co., Ltd.", address.CompanyName);
						AssertEquals("on behalf of Hitachi, Ltd., Power S", address.AddressLine1);
						AssertEquals("6 Kanda-Surugadai 4-Chome, Chiyoda- Tokyo, 101-8010 Japan Tel:+81-3-4564-6144 Fax:+81-3-3258-", address.AddressLine2);

						AssertEquals("CHINAEMTYO", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("China Emoto Construction Co., Ltd.", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("on behalf of Hitachi, Ltd., Power S", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("6 Kanda-Surugadai 4-Chome, Chiyoda- Tokyo, 101-8010 Japan Tel:+81-3-4564-6144 Fax:+81-3-3258-", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CED);
						AssertEquals("HITAPROBNE", address.AddressCode);
						AssertEquals("Hitachi Australia Project Office (B", address.CompanyName);
						AssertEquals("Suite 9A, 19 Lang Parade", address.AddressLine1);
						AssertEquals("Milton QLD 4046 Australia", address.AddressLine2);

						AssertEquals("HITAPROBNE", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Hitachi Australia Project Office (B", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Suite 9A, 19 Lang Parade", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Milton QLD 4046 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.NPP);
						AssertEquals("SAMEAS", address.AddressCode);
						AssertEquals("Same As Consignee", address.CompanyName);
						AssertEquals("", address.AddressLine1);
						AssertEquals("", address.AddressLine2);

						AssertEquals("SAMEAS", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Same As Consignee", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CEG);
						AssertEquals("HITAPROBNE", address.AddressCode);
						AssertEquals("Hitachi Australia Project Office (B", address.CompanyName);
						AssertEquals("Suite 9A, 19 Lang Parade", address.AddressLine1);
						AssertEquals("Milton QLD 4046 Australia", address.AddressLine2);

						AssertEquals("HITAPROBNE", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Hitachi Australia Project Office (B", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Suite 9A, 19 Lang Parade", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Milton QLD 4046 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.BKD);
						AssertEquals("HITACHITYO", address.AddressCode);
						AssertEquals("Hitachi (TYO)", address.CompanyName);
						AssertEquals("R-Bldg. East", address.AddressLine1);
						AssertEquals("9-1, Nihonbashi-Hakozaki-cho, Chuo- Tokyo 103-0015 Japan", address.AddressLine2);

						AssertEquals("HITACHITYO", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Hitachi (TYO)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("R-Bldg. East", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("9-1, Nihonbashi-Hakozaki-cho, Chuo- Tokyo 103-0015 Japan", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}
				}

				{
					Xsd.AgencyBillOfLading bol = bols.AgencyBillOfLading[1];
					AssertEquals("NKGBNE01P5FF105A", bol.BillNumber);

					AssertEquals(new ZDateTime(2005, 03, 17, 0, 0, 0), bol.Sailing.ETA);
					AssertEquals(new ZDateTime(2005, 03, 15, 0, 0, 0), bol.Sailing.ETD);

					AssertEquals("9231145", bol.Sailing.LloydsNo);
					AssertEquals("Cape Darnley", bol.Sailing.VesselName);
					AssertEquals("01P5S", bol.Sailing.VoyageNo);

					AssertEquals("", bol.BookingReference);
					AssertEquals("", bol.ShippersReference);
					AssertEquals("", bol.InterimReceipt);

					AssertEquals("ONE 20'HC CONTAINER SHIPPER'S LOAD, COUNT & SEAL FREIGHT PREPAID PEBBLES FREIGHT PREPAID FREIGHT PREPAID IN NANJING 20 BAGS 19,525.00KGS 15.000CBM", bol.Description);
					AssertEquals("N/M CN:GESU2856217/041739/20'HC(CY/CY)", bol.MarksAndNumbers);

					AssertEquals("PPD", bol.INCOTerm);
					AssertEquals("", bol.ChargesApply);

					//AssertEquals(false, bol.CustomsEntryNumber.IsSpecified); TODO

					AssertEquals(20, bol.PackageSummary.NumberOfPacks);
					AssertEquals(Core.Constants.PkgUnit.Bag, bol.PackageSummary.PackType);
					AssertEquals("KG", bol.PackageSummary.Weight.DimensionType);
					AssertEquals(new ZDecimal(19530.0), bol.PackageSummary.Weight.Value);
					AssertEquals("M3", bol.PackageSummary.Volume.DimensionType);
					AssertEquals(new ZDecimal(15.0), bol.PackageSummary.Volume.Value);

					AssertEquals(ZDateTime.Empty, bol.IssueDate);
					AssertEquals("", bol.ShippedOnBoard.Code);
					AssertEquals(ZDateTime.Empty, bol.ShippedOnBoard.Date);
					AssertEquals((byte)0, bol.OriginalBills);
					AssertEquals((byte)0, bol.CopyBills);

					AssertEquals(Xsd.ContainerMode.FCL, bol.CargoType);

					AssertEquals("CNSHA", bol.Load.Value);
					AssertEquals("CNNKG", bol.Origin.Value);
					AssertEquals("AUBNE", bol.Discharge.Value);
					AssertEquals("AUBNE", bol.Destination.Value);

					AssertEquals(1, bol.Containers.Count);
					AssertEquals("GESU2856217", bol.Containers[0].ContainerNumber);
					AssertEquals("20DH", bol.Containers[0].ContainerType.ContainerCode);
					AssertEquals(false, bol.Containers[0].IsShipperOwnedContainer);
					AssertEquals(Xsd.ContainerMode.FCL, bol.Containers[0].PackingMode);
					AssertEquals(new ZDecimal(2750.0), bol.Containers[0].Weight);
					AssertEquals("", bol.Containers[0].Seal);
					AssertEquals(false, bol.Containers[0].IsEmptyContainer);

					AssertEquals(1, bol.Packages.Count);
					AssertEquals(20, bol.Packages[0].NumberOfPacks);
					AssertEquals(Core.Constants.PkgUnit.Bag, bol.Packages[0].PackType);
					AssertEquals("KG", bol.Packages[0].Weight.DimensionType);
					AssertEquals(new ZDecimal(16780.0), bol.Packages[0].Weight.Value);
					AssertEquals("M3", bol.Packages[0].Volume.DimensionType);
					AssertEquals(new ZDecimal(15.0), bol.Packages[0].Volume.Value);
					//AssertEquals(false, bol.Packages[0].HazardousGoods); TODO
					AssertEquals("pebbles", bol.Packages[0].GoodsDescription);
					AssertEquals("", bol.Packages[0].MarksAndNumbers);
					AssertEquals("GESU2856217", bol.Packages[0].ContainerNumber);

					AssertEquals(5, bol.Addresses.DocAddress.Count);
					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CRD);
						AssertEquals("CHINAPENKG", address.AddressCode);
						AssertEquals("China Peace Corporation (NKG)", address.CompanyName);
						AssertEquals("330 Zhongshan Road", address.AddressLine1);
						AssertEquals("Nanjing China", address.AddressLine2);

						AssertEquals("CHINAPENKG", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("China Peace Corporation (NKG)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("330 Zhongshan Road", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Nanjing China", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CED);
						AssertEquals("TOORDERSHI", address.AddressCode);
						AssertEquals("To Order Of the Shipper", address.CompanyName);
						AssertEquals("", address.AddressLine1);
						AssertEquals("", address.AddressLine2);

						AssertEquals("TOORDERSHI", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("To Order Of the Shipper", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.NPP);
						AssertEquals("KASHPEBBNE", address.AddressCode);
						AssertEquals("Kashmiri Pebbles (BNE)", address.CompanyName);
						AssertEquals("Shed 46, 60 Bellevue Avenue", address.AddressLine1);
						AssertEquals("Enoggera QLD 4053 Australia", address.AddressLine2);

						AssertEquals("KASHPEBBNE", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Kashmiri Pebbles (BNE)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Shed 46, 60 Bellevue Avenue", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Enoggera QLD 4053 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CEG);
						AssertEquals("KASHPEBBNE", address.AddressCode);
						AssertEquals("Kashmiri Pebbles (BNE)", address.CompanyName);
						AssertEquals("Shed 46, 60 Bellevue Avenue", address.AddressLine1);
						AssertEquals("Enoggera QLD 4053 Australia", address.AddressLine2);

						AssertEquals("KASHPEBBNE", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Kashmiri Pebbles (BNE)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Shed 46, 60 Bellevue Avenue", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Enoggera QLD 4053 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.BKD);
						AssertEquals("CHINAPENKG", address.AddressCode);
						AssertEquals("China Peace Corporation (NKG)", address.CompanyName);
						AssertEquals("330 Zhongshan Road", address.AddressLine1);
						AssertEquals("Nanjing China", address.AddressLine2);

						AssertEquals("CHINAPENKG", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("China Peace Corporation (NKG)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("330 Zhongshan Road", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("Nanjing China", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}
				}

				{
					Xsd.AgencyBillOfLading bol = bols.AgencyBillOfLading[2];
					AssertEquals("SHABNE01P5FF901A", bol.BillNumber);

					AssertEquals(new ZDateTime(2005, 03, 17, 0, 0, 0), bol.Sailing.ETA);
					AssertEquals(new ZDateTime(2005, 03, 15, 0, 0, 0), bol.Sailing.ETD);

					AssertEquals("9231145", bol.Sailing.LloydsNo);
					AssertEquals("Cape Darnley", bol.Sailing.VesselName);
					AssertEquals("01P5S", bol.Sailing.VoyageNo);

					AssertEquals("", bol.BookingReference);
					AssertEquals("", bol.ShippersReference);
					AssertEquals("", bol.InterimReceipt);

					AssertEquals("20 X 20FT SOC 40 UNITS X 8FT EMPTY SOC UNITS RWMU000006 RWMU000050 RWMU000055 RWMU000057 RWMU000083 RWMU000134 RWMU000147 RWMU000148 RWMU000149 RWMU000151 RWMU000152 RWMU000153 RWMU000154 RWMU000159 RWMU000160 RWMU000161 RWMU000162 RWMU000163 RWMU000", bol.Description);
					AssertEquals("n/m", bol.MarksAndNumbers);

					AssertEquals("CLT", bol.INCOTerm);
					AssertEquals("", bol.ChargesApply);

					//AssertEquals(false, bol.CustomsEntryNumber.IsSpecified); TODO

					AssertEquals(20, bol.PackageSummary.NumberOfPacks);
					AssertEquals(Core.Constants.PkgUnit.Pallet, bol.PackageSummary.PackType);
					AssertEquals("KG", bol.PackageSummary.Weight.DimensionType);
					AssertEquals(new ZDecimal(84400.0), bol.PackageSummary.Weight.Value);
					AssertEquals("M3", bol.PackageSummary.Volume.DimensionType);
					AssertEquals(new ZDecimal(0.0), bol.PackageSummary.Volume.Value);

					AssertEquals(ZDateTime.Empty, bol.IssueDate);
					AssertEquals("", bol.ShippedOnBoard.Code);
					AssertEquals(ZDateTime.Empty, bol.ShippedOnBoard.Date);
					AssertEquals((byte)0, bol.OriginalBills);
					AssertEquals((byte)0, bol.CopyBills);

					AssertEquals(Xsd.ContainerMode.FCL, bol.CargoType);

					AssertEquals("CNSHA", bol.Load.Value);
					AssertEquals("CNSHA", bol.Origin.Value);
					AssertEquals("AUBNE", bol.Discharge.Value);
					AssertEquals("AUBNE", bol.Destination.Value);

					AssertEquals(20, bol.Containers.Count);

					string[] containerNumbers = {
						"RWMU000006",
						"RWMU000050",
						"RWMU000055",
						"RWMU000057",
						"RWMU000083",
						"RWMU000134",
						"RWMU000147",
						"RWMU000148",
						"RWMU000149",
						"RWMU000151",
						"RWMU000152",
						"RWMU000153",
						"RWMU000154",
						"RWMU000159",
						"RWMU000160",
						"RWMU000161",
						"RWMU000162",
						"RWMU000163",
						"RWMU000165",
						"RWMU000166"
					};

					bool?[] shipperOwned = {
						true,
						true,
						true,
						false,
						true,
						true,
						null,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true,
						true
					};

					for (int i = 0; i < containerNumbers.Length; i++)
					{
						Xsd.Container container = FindContainer(bol.Containers, containerNumbers[i]);

						AssertEquals(containerNumbers[i], container.ContainerNumber);
						AssertEquals("20DC", container.ContainerType.ContainerCode);

						if (shipperOwned[i].HasValue)
						{
							AssertEquals(string.Format("shipper owned specified ({0})", i), true, container.IsShipperOwnedContainerSpecified);
							AssertEquals(string.Format("shipper owned ({0})", i), shipperOwned[i], container.IsShipperOwnedContainer);
						}
						else
						{
							AssertEquals(string.Format("shipper owned specified ({0})", i), false, container.IsShipperOwnedContainerSpecified);
						}

						AssertEquals(Xsd.ContainerMode.FCL, container.PackingMode);
						AssertEquals(new ZDecimal(2500.0), container.Weight);
						AssertEquals("", container.Seal);
						AssertEquals(true, container.IsEmptyContainer);
					}

					AssertEquals(0, bol.Packages.Count); // Hmm, we don't import any for empties
					AssertEquals(5, bol.Addresses.DocAddress.Count);
					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CRD);
						AssertEquals("NANTONGCN", address.AddressCode);
						AssertEquals("Nantong CIMC Special Transportation", address.CompanyName);
						AssertEquals("", address.AddressLine1);
						AssertEquals("", address.AddressLine2);

						AssertEquals("NANTONGCN", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Nantong CIMC Special Transportation", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CED);
						AssertEquals("ROYALWOSYD", address.AddressCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.CompanyName);
						AssertEquals("Level 2, Suite 202,", address.AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressLine2);

						AssertEquals("ROYALWOSYD", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Level 2, Suite 202,", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.NPP);
						AssertEquals("SAMEAS", address.AddressCode);
						AssertEquals("Same As Consignee", address.CompanyName);
						AssertEquals("", address.AddressLine1);
						AssertEquals("", address.AddressLine2);

						AssertEquals("SAMEAS", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Same As Consignee", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.CEG);
						AssertEquals("ROYALWOSYD", address.AddressCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.CompanyName);
						AssertEquals("Level 2, Suite 202,", address.AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressLine2);

						AssertEquals("ROYALWOSYD", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Level 2, Suite 202,", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					{
						Xsd.DocAddress address = FindAddress(bol.Addresses.DocAddress, Xsd.DocAddressAddressType.BKD);
						AssertEquals("ROYALWOSYD", address.AddressCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.CompanyName);
						AssertEquals("Level 2, Suite 202,", address.AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressLine2);

						AssertEquals("ROYALWOSYD", address.AddressReference.Organisation.OwnerCode);
						AssertEquals("Royal Wolf Trading (SYD)", address.AddressReference.Organisation.OrganisationDetails.Name);
						AssertEquals(1, address.AddressReference.Organisation.OrganisationDetails.Addresses.Count);
						AssertEquals("Level 2, Suite 202,", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
						AssertEquals("22-28 Edgeworth David Road, Hornsby NSW 2027 Australia", address.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
					}

					AssertEquals(2, bol.Billing.ChargeLines.Count);
					{
						Xsd.ChargeLine line = FindChargeLine(bol.Billing.ChargeLines, "AAAAAAAA");
						AssertEquals("AAAAAAAA", line.ChargeCode);
						AssertEquals(false, line.Collect);
						AssertEquals("WWW", line.OSSellAmount.CurrencyCode);
						AssertEquals(1230.000m, line.OSSellAmount.Value);
					}

					{
						Xsd.ChargeLine line = FindChargeLine(bol.Billing.ChargeLines, "BBBBBBBB");
						AssertEquals("BBBBBBBB", line.ChargeCode);
						AssertEquals(true, line.Collect);
						AssertEquals("AUD", line.OSSellAmount.CurrencyCode);
						AssertEquals(4320.000m, line.OSSellAmount.Value);
					}
				}
			}
		}

		public void TestErrorMessageOnInvalidBOLFile()
		{
			var fullTestPathOfInvalidFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("ImportInvalidBOLFile.txt"));
			using (TextReader reader = new StreamReader(fullTestPathOfInvalidFile))
			{
				INotifications notification = new NotificationBuffer();
				ToBillOfLadingConverter converter = new ToBillOfLadingConverter(notification, Factory);

				Xsd.AgencyBillsOfLading bols = new Xsd.AgencyBillsOfLading();

				INotification[] notificationEvents = ((NotificationBuffer)notification).Events;

				AssertEquals("Precondition: Events has no notifications", 0, notificationEvents.Length);
				converter.ImportFlatFile(bols, new ShipnetFlatFileFormat(), reader);

				notificationEvents = ((NotificationBuffer)notification).Events;
				AssertEquals("Events has one notification", 1, notificationEvents.Length);

				Assert("Error message for invalid file " + fullTestPathOfInvalidFile, notificationEvents[0].Message.Contains("Company name is invalid on line 149:"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		Xsd.Container FindContainer(Xsd.ContainerCollection containers, string containerNumber)
		{
			foreach (Xsd.Container container in containers)
			{
				if (container.ContainerNumber == containerNumber)
				{
					return container;
				}
			}
			Fail("Container not found: " + containerNumber);
			return null;
		}

		Xsd.DocAddress FindAddress(Xsd.DocAddressCollection addresses, Xsd.DocAddressAddressType addressType)
		{
			foreach (Xsd.DocAddress address in addresses)
			{
				if (address.AddressTypeSpecified && address.AddressType == addressType)
				{
					return address;
				}
			}
			Fail(string.Format("Address of type {0} not found.", addressType.ToString()));
			return null;
		}

		Xsd.ChargeLine FindChargeLine(Xsd.ChargeLineCollection lines, string chargeCode)
		{
			foreach (Xsd.ChargeLine line in lines)
			{
				if (line.ChargeCode == chargeCode)
				{
					return line;
				}
			}

			Fail(string.Format("Charge line with charge code {0} not found.", chargeCode));
			return null;
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.TestFiles." + fileName;
	}
}
