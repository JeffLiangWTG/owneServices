using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	[DefaultField("JobNumber")]
	public class GateTransportWrapper : GenericWrapper
	{
		internal GateTransportWrapper(GateTransport gateTransportBO, GateTransportCFSDetail gateTransportCFSDetailBO, BusinessObjectFactory factory)
			: base(gateTransportBO, factory)
		{
			this.gateTransportBO = gateTransportBO ?? factory.GetNull<GateTransport>();
			this.gateTransportCFSDetailBO = gateTransportCFSDetailBO ?? factory.GetNull<GateTransportCFSDetail>();
		}

		public static GateTransportWrapper NewWithCYDetail(GateTransport gateTransportBO, BusinessObjectFactory factory)
		{
			return new GateTransportWrapper(
				gateTransportBO: gateTransportBO ?? factory.GetNull<GateTransport>(),
				gateTransportCFSDetailBO: factory.GetNull<GateTransportCFSDetail>(),
				factory: factory);
		}

		public static GateTransportWrapper NewWithCFSDetail(GateTransport gateTransportBO, GateTransportCFSDetail gateTransportCFSDetailBO, BusinessObjectFactory factory)
		{
			return new GateTransportWrapper(
				gateTransportBO: gateTransportBO ?? factory.GetNull<GateTransport>(),
				gateTransportCFSDetailBO: gateTransportCFSDetailBO ?? factory.GetNull<GateTransportCFSDetail>(),
				factory: factory);
		}

		readonly GateTransport gateTransportBO;
		readonly GateTransportCFSDetail gateTransportCFSDetailBO;

		public ZString JobNumber => gateTransportBO.GTT_JobNumber;

		public ZDateTime GateInDate => gateTransportBO.GTT_TimeIn;

		public ZDateTime GateOutDate => gateTransportBO.GTT_TimeOut;

		public ZString DriverName => gateTransportBO.GTT_DriverName;

		public ZString DriverLicense => gateTransportBO.GTT_DriverLicence;

		public ZString VehicleRegistration => gateTransportBO.GTT_VehicleRegistration;

		public ZString SystemCreateUserName
		{
			get
			{
				var result = ZString.Empty;

				var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, gateTransportBO.GTT_SystemCreateUser);
				if (staff != null)
				{
					result = staff.GS_FullName;
				}

				return result;
			}
		}

		public OrganisationWrapper TransportCompany
		{
			get
			{
				if (transportCompany == null)
				{
					transportCompany = new OrganisationWrapper(OrganisationUsageType.TransportCo, gateTransportBO.TransportCompanyDocumentaryAddress, Factory);
				}
				return transportCompany;
			}
		}
		OrganisationWrapper transportCompany;

		public DocBranch Branch
		{
			get
			{
				if (branch == null)
				{
					branch = DocBranch.New(gateTransportBO.GTT_GB_Branch, Factory);
				}

				return branch;
			}
		}
		DocBranch branch;

		public GateTransportCFSDetailWrapperCollection GateTransportCFSDetails
		{
			get
			{
				var result = new GateTransportCFSDetailWrapperCollection(Factory);
				if (!gateTransportCFSDetailBO.IsNull)
				{
					result.Add(new GateTransportCFSDetailWrapper(gateTransportCFSDetailBO, Factory));
				}
				else if (!gateTransportBO.IsNull)
				{
					foreach (var gateTransportDetail in gateTransportBO.GateTransportCFSDetails)
					{
						result.Add(new GateTransportCFSDetailWrapper(gateTransportDetail, Factory));
					}
				}
				return result;
			}
		}
	}
}
