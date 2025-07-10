using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionLookups : AutoBMBoardSectionLookups
	{
		public BMBoardSectionLookups(AutoBMBoardSection parent)
			: base(parent)
		{
		}

		new BMBoardSection Parent
		{
			get { return (BMBoardSection)base.Parent; }
		}

		#region OrientationList

		public OrientationList OrientationList
		{
			get { return Factory.GetCachedValue<OrientationList>(); }
		}

		#endregion

		#region Components

		public BMComponentCollection Components
		{
			get
			{
				var collection = new BMComponentCollection(Factory, new ZQuery(new ZQuery(BMComponentSchema.FC_FC_ParentComponent, null)));
				var component = Parent?.Component;
				var boardSystemPK = Parent?.SectionConfiguration?.Board?.MB_FS_System;
				collection.AddComponentLookupFilterBusinessObjectDefaults(component, boardSystemPK);
				return collection;
			}
		}

		public virtual BMBoardCollection Boards
		{
			get { return new BMBoardCollection(Factory); }
		}

		#endregion

		#region ColorList

		public CodeDescriptionPairList ColorList
		{
			get { return Factory.GetCachedValue("Enterprise.ZArchitecture.Core.ColorList", () => new ColorList()); }
		}

		#endregion

		#region SectionTypes

		public CodeDescriptionPairList SectionTypes
		{
			get
			{
				var sectionTypeList = Factory.GetCachedValue<SectionTypeList>();
				var sectionTypes = new CodeDescriptionPairList(sectionTypeList);

				// Using IsWinzor flag as Non GUI projects are not complied with WINZOR directive
				if (Globals.IsWinzor)
				{
					sectionTypes.RemoveCode(BMConstants.NetworkDiagramSectionType);
				}

				return sectionTypes;
			}
		}

		#endregion
	}
}
