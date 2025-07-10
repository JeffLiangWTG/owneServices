using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public abstract class CcsukTransmissionMessageFunction : Customs.Business.CusdecMessageFunction
	{
		public abstract string MessageType { get; }
		public abstract string MessageSubType { get; }
		public abstract class CUSCAR : CcsukTransmissionMessageFunction
		{
			public override string MessageType
			{
				get { return Code; }
			}
			public const string Code = "CAR";

			public class FRI : CUSCAR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FRI";

				public class UFO : FRI
				{ }
			}

			public class FRX : CUSCAR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FRX";
			}

			public class FRC : CUSCAR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FRC";
			}

			public class FCS : CUSCAR
			{
				public FCS(NonPersistentSplitLineCollection splits)
				{
					this.Splits = splits;
				}
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FCS";
				public NonPersistentSplitLineCollection Splits { get; private set; }
			}
		}

		public abstract class CIM : CcsukTransmissionMessageFunction
		{
			public override string MessageType
			{
				get { return Code; }
			}
			public const string Code = "CIM";

			public class FSA : CIM
			{
				public FSA(ZString osiText, EDIMessage incomingMessage)
				{
					OtherShipmentInformationLine = osiText;
					IncomingMessage = incomingMessage;
				}

				public FSA(ZString osiText, EDIMessage incomingMessage, string commonAccessReference)
					: this(osiText, incomingMessage)
				{
					CommonAccessReference = commonAccessReference;
				}

				public override string MessageSubType
				{
					get { return SubCode; }
				}
				public const string SubCode = "FSA";
				public readonly ZString OtherShipmentInformationLine;
				public readonly EDIMessage IncomingMessage;
				public readonly ZString CommonAccessReference;

				public class OSI : FSA
				{
					public OSI(ZString osiText, EDIMessage incomingMessage, ZString commonAccessReference)
						: base(osiText, incomingMessage, commonAccessReference)
					{
					}
					public string MessageFsaType { get { return FsaSubCode; } }
					public const string FsaSubCode = "OSI";
				}
			}

			public class FRN : CIM
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FRN";

				public FRN(ZString newAgent)
				{
					NewAgent = newAgent.Left(3).ToUpper();
				}

				public ZString NewAgent { get; set; }
			}

			public class FCS : CIM
			{
				public FCS(NonPersistentSplitLineCollection splits)
				{
					Splits = splits;
				}
				public override string MessageSubType { get { return "FCS"; } }
				public NonPersistentSplitLineCollection Splits { get; private set; }
			}

			public class DRP : CIM
			{
				public DRP(DropOffWrapper wrapper)
				{
					Wrapper = wrapper;
				}
				public override string MessageSubType { get { return Subcode; } }
				public const string Subcode = "DRP";
				public DropOffWrapper Wrapper { get; private set; }
			}

			public class FRD : CIM
			{
				public FRD(NonPersistentSplitsAndFlightData splitsAndFlightData)
				{
					SplitsAndFlightData = splitsAndFlightData;
				}
				public override string MessageSubType { get { return SubCode; } }
				public NonPersistentSplitsAndFlightData SplitsAndFlightData { get; private set; }
				public const string SubCode = "FRD";
			}

			public class FSR : CIM
			{
				public override string MessageSubType { get { return SubCode; } }
				public const string SubCode = "FSR";
			}
		}

		public abstract class CUKFSR : CcsukTransmissionMessageFunction
		{
			public override string MessageType
			{
				get { return Code; }
			}
			public const string Code = "FSR";

			public class FSA : CUKFSR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FSA";
			}

			public class FsaWithoutShed : FSA
			{
			}

			public class FsaForExport : FSA
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public new const string Subcode = "EXP";
			}

			public class FsaForExportWithoutShed : FsaWithoutShed
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public new const string Subcode = "EXP";
			}

			public class FSN : CUKFSR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FSN";
			}

			public class FsaWithUpdate : CUKFSR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "FAU";
			}

			public class StandaloneFsrEnquiry : CUKFSR
			{
				public override string MessageSubType
				{
					get { return Subcode; }
				}
				public const string Subcode = "ENQ";
			}
		}

		public abstract class CUSDEC : CcsukTransmissionMessageFunction
		{
			public override string MessageType
			{
				get { return Code; }
			}

			public const string Code = "CDC";

			public class IAR : CUSDEC
			{
				public override string MessageSubType { get { return "IAR"; } }
			}
			public class ISR : CUSDEC
			{
				public override string MessageSubType { get { return "ISR"; } }
			}
			public class TSR : CUSDEC
			{
				public override string MessageSubType { get { return "TSR"; } }
			}
			public class FBK : CUSDEC
			{
				public override string MessageSubType { get { return "FBK"; } }
			}

			public CusUnderbond CusUnderbond { get; set; }
		}

		public class CONTRL : CcsukTransmissionMessageFunction
		{
			public CONTRL(EDIMessage inboundEdiMessageForAuditing, UNHSegment unhSegment, string actionCoded, string errorCoded, ZString freeText)
			{
				InboundMessage = inboundEdiMessageForAuditing;
				UnhSegment = unhSegment;
				ActionCoded = actionCoded;
				ErrorCoded = errorCoded;
				FreeText = freeText;
			}

			public const string Code = "CTL";

			public override string MessageSubType
			{
				get { return ""; }
			}

			public override string MessageType
			{
				get { return "CTL"; }
			}
			public EDIMessage InboundMessage { get; private set; }
			public UNHSegment UnhSegment { get; private set; }
			public ZString ActionCoded { get; private set; }
			public ZString ErrorCoded { get; private set; }
			public ZString FreeText { get; private set; }
		}

		/// <summary>
		/// Good 2 Go - export e-Fallback
		/// </summary>
		public class CUKG2G : CcsukTransmissionMessageFunction
		{
			public CUKG2G()
			{
			}

			public const string Code = "G2G";

			public override string MessageSubType
			{
				get { return ""; }
			}

			public override string MessageType
			{
				get { return Code; }
			}
		}
	}
}
