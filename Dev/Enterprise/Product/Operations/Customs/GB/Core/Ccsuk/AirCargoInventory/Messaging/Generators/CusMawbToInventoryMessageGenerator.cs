using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public abstract class CusAwbToInventoryMessageGenerator
	{
		internal static CusAwbToInventoryMessageGenerator New(CcsukTransmissionMessageFunction transmissionMessageFunction, ICcsukCusAwb cusAwb, ErrorCollector ec)
		{
			if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR)
			{
				if (cusAwb != null)
				{
					if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR.FRI.UFO)
					{
						return new CUSCARGeneratorFRIUFO(cusAwb, ec);
					}

					if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR.FRI)
					{
						return new CUSCARGeneratorFRI(cusAwb, ec);
					}

					if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR.FRX)
					{
						return new CUSCARGeneratorFRX(cusAwb, ec);
					}

					if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR.FRC)
					{
						return new CUSCARGeneratorFRC(cusAwb, ec);
					}

					if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSCAR.FCS)
					{
						return new CUSCARGeneratorFCS(cusAwb, ec, ((CcsukTransmissionMessageFunction.CUSCAR.FCS)transmissionMessageFunction).Splits);
					}
				}
			}
			else if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUKFSR)
			{
				return new CukFsrCreator(cusAwb, transmissionMessageFunction);
			}
			else if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CIM)
			{
				if (cusAwb == null)
				{
					return new FsrFsaCycleNoRecordFoundCimWrapper((CcsukTransmissionMessageFunction.CIM.FSA)transmissionMessageFunction);
				}
				else
				{
					return new CusAwbToCimWrapper(transmissionMessageFunction, cusAwb, ec);
				}
			}
			else if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CUSDEC)
			{
				var tmfAsCusdec = transmissionMessageFunction as CcsukTransmissionMessageFunction.CUSDEC;
				if (tmfAsCusdec is CcsukTransmissionMessageFunction.CUSDEC.FBK)
				{
					return new CusdecGeneratorFBK(cusAwb, ec, tmfAsCusdec.CusUnderbond);
				}
				else if (tmfAsCusdec is CcsukTransmissionMessageFunction.CUSDEC.IAR)
				{
					return new CusdecGeneratorIAR(cusAwb, ec, tmfAsCusdec.CusUnderbond);
				}
				else if (tmfAsCusdec is CcsukTransmissionMessageFunction.CUSDEC.ISR)
				{
					return new CusdecGeneratorISR(cusAwb, ec, tmfAsCusdec.CusUnderbond);
				}
				else if (tmfAsCusdec is CcsukTransmissionMessageFunction.CUSDEC.TSR)
				{
					return new CusdecGeneratorTSR(cusAwb, ec, tmfAsCusdec.CusUnderbond);
				}
			}
			else if (transmissionMessageFunction is CcsukTransmissionMessageFunction.CONTRL)
			{
				return new OutboundContrlGenerator((CcsukTransmissionMessageFunction.CONTRL)transmissionMessageFunction);
			}

			throw new NotImplementedException("Cannot make that message type for that object. How=" + transmissionMessageFunction.GetType().Name);
		}

		protected CusAwbToInventoryMessageGenerator(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		protected CusAwbToInventoryMessageGenerator(ICcsukCusAwb cusAwb)
			: this(cusAwb.Factory)
		{
			this.Awb = cusAwb;
		}

		public abstract string MakeMessageText();

		public abstract ZString MessageInterpretation { get; }

		public virtual ZString RecipientPima
		{
			get { return CcsukEdiMessageDiverter.PimaForCommunityDatabase; }
		}

		internal EDIMessage GetNewMessage()
		{
			return GetNewMessageCore();
		}

		protected virtual EDIMessage GetNewMessageCore()
		{
			return Awb.Messages.AddNew();
		}

		internal ZString SenderPima
		{
			get { return SenderPimaCore; }
		}

		protected virtual ZString SenderPimaCore
		{
			get { return Awb.Profile; }
		}

		internal BusinessObjectFactory Factory { get; private set; }

		protected ICcsukCusAwb Awb { get; private set; }
	}
}
