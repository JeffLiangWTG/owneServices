using System;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC043A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC043AProcessor : DTBaseProcessor<Cc043AType>
	{
		public DTCC043AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC043A processor";

		protected override ZString GetNewMessageStatus(Cc043AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewArrivalStatus(Cc043AType messageObject) => NctsTransitStatusList.Codes.UnloadingPermissionGranted;

		protected override ZString GetMessageInterpretation(Cc043AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetArrivalStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}

		protected override void UpdateNCTSHeader(Cc043AType messageObject)
		{
			base.UpdateNCTSHeader(messageObject);

			var ie43Wrapper = GetIE43Wrapper(messageObject);
			if (ie43Wrapper != null)
			{
				new NctsIE43ArrivalItemsParser(NctsHeader, ie43Wrapper).Parse();
			}

			if (NctsHeader.ArrivalMovementHeader != null)
			{
				NctsHeader.ResetUnloadedValues();
				NctsHeader.UnloadingMovementHeader.ResetUnloadedGoodsItem(NctsHeader.ArrivalMovementHeader.GoodsItems);
			}
		}

		IE43Wrapper GetIE43Wrapper(Cc043AType messageObject)
		{
			IE43Wrapper wrapper = null;
			if (messageObject.Heahea != null)
			{
				wrapper = new IE43Wrapper
				{
					MeansOfTransportAtDepartureIdentity = messageObject.Heahea.IdeOfMeaOfTraAtDhea78,
					MeansOfTransportAtDepartureNationality = messageObject.Heahea.NatOfMeaOfTraAtDhea80,
					TotalGrossMass = (ZDecimal)messageObject.Heahea.TotGroMasHea307,
					TotalGrossMassUQ = Core.Constants.Weight.Kilograms,
					Seals = GetSeals(messageObject),
					GoodsItems = GetGoodsItems(messageObject)
				};
			}
			return wrapper;
		}

		ZString[] GetSeals(Cc043AType messageObject)
		{
			if (messageObject.Seainfsli?.Seaidsid != null)
			{
				return (from sel in messageObject.Seainfsli.Seaidsid select (ZString)sel.SeaIdeSid1).ToArray();
			}
			return null;
		}

		IE43GoodsItemWrapper[] GetGoodsItems(Cc043AType messageObject)
		{
			if (messageObject.Gooitegds != null)
			{
				return (from goodsItem in messageObject.Gooitegds
						select new IE43GoodsItemWrapper
						{
							ItemNumber = ZShort.ParseSafe(goodsItem.IteNumGds7, 0),
							CommodityCode = goodsItem.ComCodTarCodGds10,
							DeclarationType = goodsItem.DecTypGds15,
							DescriptionOfGoods = goodsItem.GooDesGds23,
							GrossWeight = goodsItem.GroMasGds46 ?? Decimal.Zero,
							GrossWeightUQ = Core.Constants.Weight.Kilograms,
							NetWeight = goodsItem.NetMasGds48 ?? Decimal.Zero,
							NetWeightUQ = Core.Constants.Weight.Kilograms,
							CountryOfDispatch = goodsItem.CouOfDisGds58,
							CountryOfDestination = goodsItem.CouOfDesGds59,
							ProducedDocumentsCertificates = GetProducedDocumentsCertificates(goodsItem),
							SpecialMentions = GetSpecialMentions(goodsItem),
							Containers = GetContainers(goodsItem),
							Packages = GetPackages(goodsItem),
							SgiCodes = GetSgiCodes(goodsItem)
						}).ToArray();
			}
			return null;
		}

		IE43ProducedDocumentsCertificateWrapper[] GetProducedDocumentsCertificates(GooitegdsType goodsItem)
		{
			if (goodsItem.Prodocdc2 != null)
			{
				return (from doc in goodsItem.Prodocdc2
						select new IE43ProducedDocumentsCertificateWrapper
						{
							Code = doc.DocTypDc21,
							ReferenceNumber = doc.DocRefDc23,
							Description = doc.ComOfInfDc25
						}).ToArray();
			}
			return null;
		}

		IE43SpecialMentionWrapper[] GetSpecialMentions(GooitegdsType goodsItem)
		{
			if (goodsItem.Spemenmt2 != null)
			{
				return (from sm in goodsItem.Spemenmt2
						select new IE43SpecialMentionWrapper
						{
							Code = sm.AddInfCodMt23,
							NctsExportFromEC = sm.ExpFroEcmt24ValueSpecified && sm.ExpFroEcmt24 == Flag.Item1,
							CountryCode = sm.ExpFroCouMt25
						}).ToArray();
			}
			return null;
		}

		ZString[] GetContainers(GooitegdsType goodsItem)
		{
			if (goodsItem.Connr2 != null)
			{
				return (from cnt in goodsItem.Connr2 select (ZString)cnt.ConNumNr21).ToArray();
			}
			return null;
		}

		IE43PackageWrapper[] GetPackages(GooitegdsType goodsItem)
		{
			if (goodsItem.Pacgs2 != null)
			{
				return (from pkg in goodsItem.Pacgs2
						select new IE43PackageWrapper
						{
							MarksAndNumbers = pkg.MarNumOfPacGs21,
							UnitType = pkg.KinOfPacGs23,
							UnitCount = ZLong.ParseSafe(pkg.NumOfPacGs24, 0),
							NumberOfPieces = ZLong.ParseSafe(pkg.NumOfPieGs25, 0)
						}).ToArray();
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter strings")]
		IE43SgiCodeWrapper[] GetSgiCodes(GooitegdsType goodsItem)
		{
			if (goodsItem.Sgicodsd2 != null)
			{
				return (from sgi in goodsItem.Sgicodsd2
						where sgi.SenGooCodSd22ValueSpecified
						select new IE43SgiCodeWrapper
						{
							Code = "SGI" + sgi.SenGooCodSd22.ToString().TrimStart("Item".ToCharArray()),
							Description = sgi.SenQuaSd23.ToString(CultureInfo.InvariantCulture)
						}).ToArray();
			}
			return null;
		}
	}
}
