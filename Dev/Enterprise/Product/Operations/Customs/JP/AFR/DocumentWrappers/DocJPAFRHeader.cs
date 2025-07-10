using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers
{
	[DefaultField("MasterBillNumber")]
	public class DocJPAFRHeader : DocBaseWrapper
	{
		DocJPAFRHeader(JPAFRHeader header, BusinessObjectFactory factoryToWrap)
			: base(header, factoryToWrap)
		{
			Argument.NotNull(header, "header");
		}

		public static DocJPAFRHeader New(JPAFRHeader header, BusinessObjectFactory factoryToWrap)
		{
			return header != null ? new DocJPAFRHeader(header, factoryToWrap) : null;
		}

		#region Related Business Objects

		JPAFRHeader WrappedHeader
		{
			get { return (JPAFRHeader)WrappedObject; }
		}

		RefVessel Vessel
		{
			get { return WrappedHeader == null ? null : WrappedHeader.Vessel; }
		}

		GlbBranch ReportingBranch
		{
			get { return WrappedHeader == null ? null : WrappedHeader.Branch; }
		}

		GlbCompany ReportingCompany
		{
			get { return WrappedHeader == null ? null : WrappedHeader.Company; }
		}

		#endregion

		#region Time Info

		public ZDateTime CurrentTime
		{
			get
			{
				if (currentTime.IsEmpty)
				{
					currentTime = ZDateTime.UtcNow.AddHours(9);
				}
				return currentTime;
			}
		}
		ZDateTime currentTime;

		public ZString CurrentJapanYear
		{
			get { return CurrentTime.AddYears(-2018).Year.ToString(); }
		}

		#endregion

		#region Header Info

		#region First Line of Header

		public ZString ReporterName
		{
			get
			{
				var company = ReportingCompany;
				return company != null ? company.GC_Name : ZString.Empty;
			}
		}

		public ZString BranchAddress
		{
			get
			{
				var branch = ReportingBranch;
				var result = string.Empty;
				if (branch != null)
				{
					result = string.Format("{0} {1} {2} {3} {4}", branch.GB_Address1, branch.GB_Address2, branch.GB_City, branch.GB_State, branch.Country.RN_Code);
				}
				return result;
			}
		}

		public ZString BranchPhone
		{
			get
			{
				var branch = ReportingBranch;
				return branch != null ? branch.GB_Phone_Formatted : ZString.Empty;
			}
		}

		public ZString ReporterID
		{
			get
			{
				var company = ReportingCompany;
				var reporterIDRegistryItem = JPAFRRegistry.Instance.AFRReporterIDForDocument;
				var registryReporterID = company == null ? ZString.Empty : reporterIDRegistryItem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty).ReporterID;
				return registryReporterID.IsEmpty ? reporterIDRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ReporterID : registryReporterID;
			}
		}

		#endregion

		#region Second Line of Header

		#region Vessel Info

		public ZString VesselName
		{
			get { return WrappedHeader.JPH_VesselName; }
		}

		public ZString VesselCallSign
		{
			get { return Vessel != null ? Vessel.RV_RadioCallSign : ZString.Empty; }
		}

		public ZString VesselRegisteredCountry
		{
			get { return Vessel != null ? Vessel.RV_RN_NKCountryOfReg : ZString.Empty; }
		}

		#endregion

		public ZString VoyageNumber
		{
			get { return WrappedHeader.JPH_Voyage; }
		}

		public ZString PortOfLoading
		{
			get { return WrappedHeader.JPH_RL_NKLoading; }
		}

		public ZDateTime ETD
		{
			get { return WrappedHeader.JPH_ETD; }
		}

		public ZString PortOfDischarge
		{
			get { return WrappedHeader.JPH_RL_NKDischarge; }
		}

		public ZDateTime ETA
		{
			get { return WrappedHeader.JPH_ETA; }
		}

		public ZString CarrierCode
		{
			get { return WrappedHeader.JPH_CarrierCode; }
		}

		#endregion

		public ZString MasterBillNumber
		{
			get { return WrappedHeader.JPH_MasterBillNumber; }
		}

		#endregion

		#region Collections

		public DocJPAFRContainerCollection Containers
		{
			get
			{
				var result = new DocJPAFRContainerCollection(Factory);
				Bills.OfType<DocJPAFRBills>().Where(bill => bill.Containers.Count > 0).ToList().ForEach(bill => result.AddRange(bill.Containers));
				Bills.OfType<DocJPAFRBills>().Where(bill => bill.Containers.Count == 0).ToList().ForEach(bill => result.Add(DocJPAFRContainer.NewDummy(bill, bill.Factory)));

				return result;
			}
		}

		public DocJPAFRBillsCollection Bills
		{
			get
			{
				var result = new DocJPAFRBillsCollection(Factory);
				foreach (var bill in WrappedHeader.Bills)
				{
					result.Add(DocJPAFRBills.New(bill, Factory));
				}
				return result;
			}
		}

		#endregion
	}
}
