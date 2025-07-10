using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema("UniversalSchedule.xsd"), RootElement("UniversalSchedule"),]
	public partial class Schedule : TopLevelDataObject
	{
		public Schedule()
		{
		}

		public Schedule(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		[MaxLength(3)]
		[Mandatory]
		public ZString? DataProvider { get; set; }

		[Mandatory]
		public ZBool? IsCancellation { get; set; }

		public OrganizationAddress Carrier { get; set; }  // JV_OH_Line
		public ScheduleTransport Transport { get; set; }

		public List<Loading> LoadingCollection { get; private set; }
		public List<Discharge> DischargeCollection { get; private set; }
	}
}
