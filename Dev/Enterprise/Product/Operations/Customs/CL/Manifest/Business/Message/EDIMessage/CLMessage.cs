using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLMessage : EDIMessage, Integration.Customs.CL.ICLMessage
	{
		public CLMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.GetOutgoingCLCustomsMessageNumber().GetNextFormatted(Factory);
		}

		public CLMessage OriginalMessage
		{
			get
			{
				if (IsTransmitMessage)
				{
					throw new InvalidOperationException("Original Message is only available for response messages. This message is a transmit");
				}
				if (!EM_MessageNum.IsEmpty && (fOriginalMessage == null || fOriginalMessage.EM_MessageNum != EM_MessageNum))
				{
					fOriginalMessage = new Loader(Factory).LoadTop1WithDirectionOrderByCreatTime(EM_ApplicationCode, EM_MessageNum, CLMessage.Direction.Transmit, GetExtraOriginalMessageFilter());
				}
				return fOriginalMessage;
			}
		}
		CLMessage fOriginalMessage;

		protected override IStreamFormatter MessageStreamFormatter
		{
			get { return new EDIMessageStreamFormatterForXml(); }
		}

		protected virtual ZQuery GetExtraOriginalMessageFilter() => null;

		#region Loader

#pragma warning disable IDE0001 // Simplify Names
		public new class Loader : EDIMessage.Loader
#pragma warning restore IDE0001 // Simplify Names
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CLMessage LoadTop1WithDirectionOrderByCreatTime(ZString applicationCode, ZString messageNum, ZString receiveTransmit, ZQuery extraFilter = null)
			{
				ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
				{
					OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " DESC",
				};
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, receiveTransmit);

				if (extraFilter != null)
				{
					query.AddToFilter(extraFilter);
				}
				return Factory.LoadTop1<CLMessage>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CLMessage);
		}

		#endregion

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(AsycudaBill);
		}
	}
}
