using System;
using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EuroPacificManifestImporter : ManifestImporter
	{
		public EuroPacificManifestImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implemetation

		protected override void ProcessLine(string line)
		{
			Type mapperType = GetLineMapper(line);
			if (mapperType != null)
			{
				DataLine mapper = (DataLine)mapperType.GetConstructor(new Type[] { typeof(ManifestImporter), typeof(ExportCustomsManifestHeader), typeof(string) }).Invoke(new object[] { this, GeneratedHeader, line });
				mapper.Process();
			}
		}

		protected Type GetLineMapper(string line)
		{
			if (mappers == null)
			{
				mappers = new Hashtable();
				mappers.Add("HDR", typeof(FileHeaderLine));
				mappers.Add("BHD", typeof(BookingHeaderLine));
				mappers.Add("CGR", typeof(CargoGroupLine));
				mappers.Add("CIT", typeof(CargoGroupItemsLine));
				mappers.Add("SPR", typeof(ShipperDetailsLine));
				mappers.Add("CNT", typeof(SummaryLine));
			}
			if (line.Length >= 3)
			{
				return (Type)mappers[line.Substring(0, 3)];
			}
			return null;
		}

		protected Hashtable mappers;

		#endregion
		protected sealed override BillingInterfaceName InterfaceName
		{
			get { return BillingInterfaceName.EuroPacificManifestImport; }
		}
	}
}
