using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Messaging4_1 = Enterprise.Customs.IE.EMCS.Messaging.Phase4_1;
using Version4_1 = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSInboundEDIMessage : IE.Business.InboundEDIMessage
	{
		public EMCSInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EMCSInboundEDIMessageLookups Lookups => (EMCSInboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new EMCSInboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
		}

		protected override Type DecideDataProviderType<TDataProvider>(Type xmlObjectType)
			=> DataProviderMap.GetValueOrDefault(xmlObjectType)
				?? base.DecideDataProviderType<TDataProvider>(xmlObjectType);

		public ImmutableDictionary<Type, Type> DataProviderMap => dataProviderMap ?? (dataProviderMap = ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
		{
			{ typeof(Version4_1.IE704.Ie704Type), typeof(Messaging4_1.IE704Provider) },
			{ typeof(Version4_1.IE801.Ie801Type), typeof(Messaging4_1.IE801Provider) },
			{ typeof(Version4_1.IE802.Ie802Type), typeof(Messaging4_1.IE802Provider) },
			{ typeof(Version4_1.IE803.Ie803Type), typeof(Messaging4_1.IE803Provider) },
			{ typeof(Version4_1.IE810.Ie810Type), typeof(Messaging4_1.IE810Provider) },
			{ typeof(Version4_1.IE813.Ie813Type), typeof(Messaging4_1.IE813Provider) },
			{ typeof(Version4_1.IE818.Ie818Type), typeof(Messaging4_1.IE818Provider) },
			{ typeof(Version4_1.IE819.Ie819Type), typeof(Messaging4_1.IE819Provider) },
			{ typeof(Version4_1.IE829.Ie829Type), typeof(Messaging4_1.IE829Provider) },
			{ typeof(Version4_1.IE839.Ie839Type), typeof(Messaging4_1.IE839Provider) },
			{ typeof(Version4_1.IE917.Ie917Type), typeof(Messaging4_1.IE917Provider) }
		}));
		ImmutableDictionary<Type, Type> dataProviderMap;
	}
}
