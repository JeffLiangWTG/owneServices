using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	[AllowNoStaticNew]
	public class ESDocSADHPageExport : DocSADHPage
	{
		public static ESDocSADHPageExport New(BusinessObjectFactory factory, ESCusEntryLine entryLine1)
		{
			return new ESDocSADHPageExport(factory, ESDocSADHLineExport.New(entryLine1, factory), null, null);
		}

		public static ESDocSADHPageExport New(BusinessObjectFactory factory, List<ESDocSADHLineExport> lines, int startFrom)
		{
			return new ESDocSADHPageExport(factory
				, GetElementSafe(lines, startFrom, factory)
				, GetElementSafe(lines, startFrom + 1, factory)
				, GetElementSafe(lines, startFrom + 2, factory)
			);
		}

		ESDocSADHPageExport(BusinessObjectFactory factory, ESDocSADHLineExport line1, ESDocSADHLineExport line2, ESDocSADHLineExport line3)
			: base(factory, line1, line2, line3)
		{
		}

		static ESDocSADHLineExport GetElementSafe(List<ESDocSADHLineExport> lines, int index, BusinessObjectFactory factory)
		{
			var line = index < lines.Count ? lines[index] : ESDocSADHLineExport.New(null, factory);
			return line;
		}
	}
}
