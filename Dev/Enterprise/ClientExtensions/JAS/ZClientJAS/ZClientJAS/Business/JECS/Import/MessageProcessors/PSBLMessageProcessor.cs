using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class PSBLMessageProcessor : OceanMessageProcessor
	{
		public PSBLMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(OHBLRecord); }
		}

		protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			return ProcessStandardShipment(factoryProvider, 0, notificationSubscriber);
		}
	}
}
