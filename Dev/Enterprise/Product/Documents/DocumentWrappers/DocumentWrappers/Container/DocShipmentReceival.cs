using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocShipmentReceival : DocShipment, IDocCartageAdvice
	{
		protected DocShipmentReceival(CFSShipment shipmentReceival, BusinessObjectFactory factoryToWrap)
			: base(shipmentReceival, factoryToWrap)
		{
		}

		public static new DocShipmentReceival New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<CFSShipment>(pK), factory);
		}

		public static DocShipmentReceival New(CFSShipment shipmentReceival, BusinessObjectFactory factoryToWrap)
		{
			return (shipmentReceival == null) ? null : new DocShipmentReceival(shipmentReceival, factoryToWrap);
		}

		public override string ToString()
		{
			return ShipmentNumber;
		}

		public override DocDocAddress ExportReceivingDepot
		{
			get { return DocDocAddress.New(ShipmentReceival.ExportReceivingDepot, Factory); }
		}

		public override DocSailing Sailing
		{
			get { return DocSailing.New(ShipmentReceival.Sailing, Factory); }
		}

		internal DocTransport MostInterestingTransport
		{
			get
			{
				if (mostInterestingTransport == null)
				{
					var transport = DocTransport.New(ShipmentReceival, ShipmentReceival.MostInterestingTransport, Factory);
					mostInterestingTransport = transport ?? DocTransport.New(ShipmentReceival.Sailing, Factory);
				}

				return mostInterestingTransport;
			}
		}
		DocTransport mostInterestingTransport;

		public ZBool TranshipToOtherCFS
		{
			get { return ShipmentReceival.JS_TranshipToOtherCFS; }
		}

		#region IDocCartageAdvice

		public override ZBool PrintAsContainers
		{
			get { return false; }
		}

		#endregion

		#region CFSLabel Fields

		public ZString ClientName
		{
			get { return HandledOnBehalfOfForwarder != null ? HandledOnBehalfOfForwarder.Name : ZString.Empty; }
		}

		public ZString ExportVessel
		{
			get { return (MostInterestingTransport != null) ? MostInterestingTransport.VesselName : ZString.Empty; }
		}

		public ZString ExportVoyage
		{
			get { return (MostInterestingTransport != null) ? MostInterestingTransport.VoyageFlight : ZString.Empty; }
		}

		public ZBool IsOnForwarding(DocTransport transport)
		{
			return (ShipmentAndTransportNotEmpty(transport) &&
				this.DestinationCode != transport.PortOfDischargeCode &&
				this.DestinationCode.SubstringSafe(0, 2) == transport.PortOfDischargeCode.SubstringSafe(0, 2));
		}

		public ZInt TotalNumberOfOuterPacks
		{
			get { return fTotalNumberOfOuterPacks; }
			set { fTotalNumberOfOuterPacks = value; }
		}

		public ZString PackLinePackageType
		{
			get { return fPackLinePackageType; }
			set { fPackLinePackageType = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching untranslated report name")]
		public DocCFSShipmentCollection CFSShipments
		{
			get
			{
				switch (ReportName.Trim().ToUpper())
				{
					case "IMPORT":
						return AddShipment(this.IsImport, LabelType.Import);
					case "TRANSHIPMENT":
						return AddShipment(this.IsTranshipment, LabelType.Transhipment);
					case "ONFORWARDING":
						return AddShipment(this.IsOnForwarding(MostInterestingTransport), LabelType.Onforwarding);
					case "CFS EXPORT LABEL":
						return AddShipment(true, LabelType.Export);
					default:
						return new DocCFSShipmentCollection(CommonShipment.Factory);
				}
			}
		}

		#endregion

		#region Implementation

		CFSShipment ShipmentReceival
		{
			get { return (CFSShipment)WrappedObject; }
		}

		ZInt fTotalNumberOfOuterPacks;
		ZString fPackLinePackageType;

		ZBool IsTranshipment
		{
			get
			{
				return ShipmentAndTransportNotEmpty(MostInterestingTransport) &&
					this.DestinationCode.SubstringSafe(0, 2) != MostInterestingTransport.PortOfDischargeCode.SubstringSafe(0, 2);
			}
		}

		new ZBool IsImport
		{
			get
			{
				return ShipmentAndTransportNotEmpty(MostInterestingTransport)
					&& DestinationLoco != null
					&& Consol != null
					&& Consol.LastDischargePort != null
					&& Consol.LastDischargePort.CountryCode == DestinationLoco.CountryCode;
			}
		}

		ZBool ShipmentAndTransportNotEmpty(DocTransport transport)
		{
			return transport != null && !this.DestinationCode.IsEmpty && !transport.PortOfDischargeCode.IsEmpty;
		}

		enum LabelType
		{
			Import,
			Onforwarding,
			Transhipment,
			Export
		}

		DocCFSShipmentCollection AddShipment(bool isAllowed, LabelType typeOfLabel)
		{
			var result = new DocCFSShipmentCollection(CommonShipment.Factory);
			if (isAllowed)
			{
				var totalPackages = CalculateNumberOfPacks();
				switch (typeOfLabel)
				{
					case LabelType.Export:
						var packType = UsePKGAsPackageTypeForLabels ? new ZString("PKG") : OuterPacksPackType;
						result = AddLabel(1, typeOfLabel, totalPackages, "", packType);
						break;

					default:
						if (CommonShipment.OuterPackLines.Count > 0)
						{
							ZString packLinePackType = "";
							foreach (PackLine line in ShipmentReceival.OuterPackLines)
							{
								packLinePackType = UsePKGAsPackageTypeForLabels ? new ZString("PKG") : line.JL_F3_NKPackType;
								result.AddRange(AddLabel(line.JL_PackageCount, typeOfLabel, totalPackages, line.JL_Calc_ContainerNum, packLinePackType));
							}
						}
						else
						{
							result = AddLabel(OuterPacks, typeOfLabel, totalPackages, "", OuterPacksPackType);
						}
						break;
				}
			}

			return result;
		}

		ZInt CalculateNumberOfPacks()
		{
			ZInt totalPacks = 0;
			if (CommonShipment.OuterPackLines.Count > 0)
			{
				foreach (PackLine line in ShipmentReceival.OuterPackLines)
				{
					totalPacks += line.JL_PackageCount;
				}
			}
			else
			{
				totalPacks = OuterPacks;
			}

			return totalPacks;
		}

		DocCFSShipmentCollection AddLabel(ZInt numberOfLabel, LabelType typeOfLabel, ZInt totalPacks, ZString containerNum, ZString packType)
		{
			DocCFSShipmentCollection result = new DocCFSShipmentCollection(CommonShipment.Factory);
			for (int i = 0; i < numberOfLabel; i++)
			{
				NonPersistentCFSShipment cfsShipment = new NonPersistentCFSShipment();
				cfsShipment.CFSContainerNumber = containerNum;
				SetShipmentSettings(cfsShipment, typeOfLabel);
				DocCFSShipment newDocCFSShipment = DocCFSShipment.New(cfsShipment, Factory);
				newDocCFSShipment.TotalPackages = totalPacks;
				newDocCFSShipment.CFSPackageType = packType;
				result.Add(newDocCFSShipment);
			}
			return result;
		}

		void SetShipmentSettings(NonPersistentCFSShipment cfsShipment, LabelType typeOfLabel)
		{
			cfsShipment.ShipmentNumber = ShipmentNumber;
			cfsShipment.CFSVessel = ExportVessel;
			cfsShipment.CFSVoyage = ExportVoyage;
			cfsShipment.ClientName = ClientName;
			cfsShipment.ETA = (Sailing != null && !Sailing.ETA.IsEmpty) ? Sailing.ETA.ToShortDateString() : "";
			cfsShipment.Dest = DestinationCode;
			cfsShipment.HBL = HouseBill;
			cfsShipment.Marks = MarksAndNumbersLine;
			if (typeOfLabel == LabelType.Onforwarding)
			{
				cfsShipment.LabelConNote = ConNote;
				cfsShipment.LabelConNoteHeading = "CON";
				cfsShipment.LabelConsignee = Consignee != null ? Consignee.Name : ZString.Empty;
				cfsShipment.LabelConsigneeHeading = "CNE";
			}
		}

		ZBool UsePKGAsPackageTypeForLabels
		{
			get
			{
				if (CommonShipment.OuterPackLines.Count > 0)
				{
					foreach (PackLine line in ShipmentReceival.OuterPackLines)
					{
						if (OuterPacksPackType != line.JL_F3_NKPackType)
						{
							return ZBool.True;
						}
					}
					return ZBool.False;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		#endregion
	}
}
