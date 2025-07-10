using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0;
using UCC6_V1 = Enterprise.Customs.IE.Messaging.UCC6.V1;

namespace Enterprise.Customs.IE.Business
{
	public class AISInboundEDIMessage : InboundEDIMessage
	{
		public AISInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new AISInboundEDIMessageLookups Lookups => (AISInboundEDIMessageLookups)base.Lookups;
		protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new AISInboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
		}

		protected override Type DecideDataProviderType<TDataProvider>(Type xmlObjectType)
			=> DataProviderMap.GetValueOrDefault(xmlObjectType)
			?? base.DecideDataProviderType<TDataProvider>(xmlObjectType);

		public ImmutableDictionary<Type, Type> DataProviderMap => dataProviderMap ??= ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
		{
			{ typeof(AIS_H7_Version1_0.IM099.Im099), typeof(UCC6_V1.IM099Provider) },
			{ typeof(AIS_H7_Version1_0.IM404.Im404), typeof(UCC6_V1.IM404Provider) },
			{ typeof(AIS_H7_Version1_0.IM405.Im405), typeof(UCC6_V1.IM405Provider) },
			{ typeof(AIS_H7_Version1_0.IM409.Im409), typeof(UCC6_V1.IM409Provider) },
			{ typeof(AIS_H7_Version1_0.IM460.Im460), typeof(UCC6_V1.IM460Provider) },
			{ typeof(AIS_H7_Version1_0.IM484.Im484), typeof(UCC6_V1.IM484Provider) },
			{ typeof(AIS_H7_Version1_0.IM416.Im416), typeof(UCC6_V1.IM416Provider) },
			{ typeof(AIS_H7_Version1_0.IM428.Im428), typeof(UCC6_V1.IM428Provider) },
			{ typeof(AIS_H7_Version1_0.IM429.Im429), typeof(UCC6_V1.IM429Provider) },
			{ typeof(AIS_H7_Version1_0.IM433.Im433), typeof(UCC6_V1.IM433Provider) },
			{ typeof(AIS_H7_Version1_0.IM451.Im451), typeof(UCC6_V1.IM451Provider) },
			{ typeof(AISVersion2_0.IM099.Im099), typeof(IM099Provider) },
			{ typeof(AISVersion2_0.IM404.Im404), typeof(IM404Provider) },
			{ typeof(AISVersion2_0.IM405.Im405), typeof(IM405Provider) },
			{ typeof(AISVersion2_0.IM409.Im409), typeof(IM409Provider) },
			{ typeof(AISVersion2_0.IM460.Im460), typeof(IM460Provider) },
			{ typeof(AISVersion2_0.IM484.Im484), typeof(IM484Provider) },
			{ typeof(AISVersion2_0.IM416.Im416), typeof(IM416Provider) },
			{ typeof(AISVersion2_0.IM428.Im428), typeof(IM428Provider) },
			{ typeof(AISVersion2_0.IM429.Im429), typeof(IM429Provider) },
			{ typeof(AISVersion2_0.IM451.Im451), typeof(IM451Provider) },
			{ typeof(AISVersion2_0.IM933.Im933), typeof(IM933Provider) },
		});

		ImmutableDictionary<Type, Type> dataProviderMap;
	}
}
