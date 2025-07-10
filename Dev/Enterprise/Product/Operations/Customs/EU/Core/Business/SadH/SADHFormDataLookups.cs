using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class SADHFormDataLookups : Customs.Business.SADH.SADHFormDataLookups
	{
		public SADHFormDataLookups(SADHFormData parent)
			: base(parent)
		{
			sADHData = parent;
			declaration = sADHData.Declaration;
		}
		readonly SADHFormData sADHData;
		readonly JobDeclaration declaration;

		public CodeDescriptionPairList EntryStyleList
		{
			get { return declaration.Lookups.GetEntryStyleList(sADHData); }
		}

		public WarehouseClientCollection LocationOfGoodsList
		{
			get { return new WarehouseClientCollection(Factory); }
		}

		public ModeOfTransportList InlandModeOfTransportList
		{
			get { return new ModeOfTransportList(); }
		}

		public CodeDescriptionPairList CommunityTransitStatusList
		{
			get
			{
				if (declaration.IsExport)
				{
					return new ExportCommunityTransitStatusList();
				}
				else if (declaration.IsImport)
				{
					return new ImportCommunityTransitStatusList();
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}
	}
}
