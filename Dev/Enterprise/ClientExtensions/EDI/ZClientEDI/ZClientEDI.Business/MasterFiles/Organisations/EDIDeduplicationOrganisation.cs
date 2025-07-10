using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common.Deduplication.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIDeduplicationOrganisation : DeduplicationOrganisation
	{
		public EDIDeduplicationOrganisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString DOH_EnterpriseId
		{
			get
			{
				var result = ZString.Empty;
				if (MasterEDIOrgHeader != null)
				{
					result = MasterEDIOrgHeader.LicenceEnterpriseID;
				}

				return result;
			}
		}

		public ZString DOH_EnterpriseCode
		{
			get
			{
				var result = ZString.Empty;
				if (MasterEDIOrgHeader != null)
				{
					result = MasterEDIOrgHeader.LicenceEnterpriseCode;
				}

				return result;
			}
		}

		public ZString DOH_CompanyCode
		{
			get
			{
				var result = ZString.Empty;
				if (MasterEDIOrgHeader != null)
				{
					result = MasterEDIOrgHeader.CompanyCode;
				}

				return result;
			}
		}

		public ZString DOH_ProductId
		{
			get
			{
				var result = ZString.Empty;
				if (MasterEDIOrgHeader != null)
				{
					var array = MasterEDIOrgHeader.ProductId?.Distinct().ToArray();
					if (array != null)
					{
						result = ZString.Join(", ", array);
					}
				}

				return result;
			}
		}

		IEDIOrgHeader MasterEDIOrgHeader => MasterOrgHeader as IEDIOrgHeader;
	}
}