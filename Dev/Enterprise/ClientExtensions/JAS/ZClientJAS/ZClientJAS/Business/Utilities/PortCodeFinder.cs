
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Utilities
{
	public class PortCodeFinder
	{
		public ZString GetPortCodeFromIATACode(BusinessObjectFactory factory, ZString iATACode)
		{
			return GetPortCodeFromIATACode(factory, iATACode, "");
		}

		public ZString GetPortCodeFromIATACode(BusinessObjectFactory factory, ZString iATACode, ZString countryCode)
		{
			ZString result = "";

			if (!iATACode.IsEmpty)
			{
				RefUNLOCO[] ports = (RefUNLOCO[])factory.Load(typeof(RefUNLOCO), GetIATAFilter(iATACode));
				if (ports.Length == 1)
				{
					result = ports[0].RL_Code;
				}
				else if (ports.Length == 0)
				{
					result = new ZString(countryCode + iATACode).Left(RefUNLOCO.Schema.RL_CodeMaxLength);
				}
				else
				{
					result = GetBestPossiblePortCodeMatch(ports, iATACode, countryCode);
				}
			}

			return result;
		}

		ZQuery GetIATAFilter(ZString iATACode)
		{
			ZQuery result = new ZQuery(RefUNLOCOSchema.RL_IATA, iATACode);
			result.OrderBy = RefUNLOCOSchema.Constants.RL_Code;
			return result;
		}

		ZString GetBestPossiblePortCodeMatch(RefUNLOCO[] ports, ZString iATACode, ZString countryCode)
		{
			ZString result = "";

			ZString portEndingWithIATACode = "";
			foreach (RefUNLOCO port in ports)
			{
				if (port.Code.EndsWith(iATACode))
				{
					portEndingWithIATACode = port.Code;
					if (port.RL_RN_NKCountryCode == countryCode)
					{
						result = port.Code;
						break;
					}
				}
			}

			return (result.IsEmpty) ? portEndingWithIATACode : result;
		}
	}
}

#region Implementation
#endregion
