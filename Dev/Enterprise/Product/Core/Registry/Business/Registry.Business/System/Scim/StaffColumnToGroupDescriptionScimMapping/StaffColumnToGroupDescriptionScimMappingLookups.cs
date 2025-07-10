using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class StaffColumnToGroupDescriptionScimMappingLookups : ZLookups
	{
		public StaffColumnToGroupDescriptionScimMappingLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StaffList
		{
			get
			{
				return Factory.GetCachedValue("StaffColumnToGroupDescriptionScimMappingLookups.StaffList",
					() =>
					{
						return StaffColumnList;
					});
			}
		}

		public static class StaffColumn
		{
			public const string CanLogin = "GS_CanLogin";
			public const string IsController = "GS_IsController";
			public const string IsRobot = "GS_IsRobot";
			public const string IsSalesRep = "GS_IsSalesRep";
			public const string IsDriver = "GS_IsDriver";
			public const string IsDevice = "GS_IsDevice";
			public const string IsDatabaseDeveloper = "IsDatabaseDeveloper";
			public const string IsReadOnlyDBUser = "IsReadOnlyDBUser";
			public const string IsBackupOperator = "IsBackupOperator";
		}

		static CodeDescriptionPairList StaffColumnList
		{
			get
			{
				CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(StaffColumn.CanLogin, Res.GetString("E49CDC10-424B-41E0-9C7C-5342D9F739CE", "Can Login"));
				codeDescriptionPairList.AddPair(StaffColumn.IsController, Res.GetString("0BC0A3A8-C86F-4D42-823F-46DDD365CA34", "Is Controller"));
				codeDescriptionPairList.AddPair(StaffColumn.IsRobot, Res.GetString("B403BB53-2665-4269-8E00-26658B7E7086", "Is Robot"));
				codeDescriptionPairList.AddPair(StaffColumn.IsSalesRep, Res.GetString("241608F1-088B-4A21-B5AA-363BE675F893", "Is Sales"));
				codeDescriptionPairList.AddPair(StaffColumn.IsDriver, Res.GetString("9FD8FDC3-FAB4-43D0-AD70-8C3C2173E034", "Is Driver"));
				codeDescriptionPairList.AddPair(StaffColumn.IsDevice, Res.GetString("574225A3-5C3D-43E3-BA17-B1E984885DF0", "Is Device"));
				codeDescriptionPairList.AddPair(StaffColumn.IsDatabaseDeveloper, Res.GetString("3FD86EE8-8787-4099-907D-B35A9005D64F", "Is Database Developer"));
				codeDescriptionPairList.AddPair(StaffColumn.IsReadOnlyDBUser, Res.GetString("C81682FE-FAC1-4EE9-83C1-C532D73939D0", "Is Read-Only DB User"));
				codeDescriptionPairList.AddPair(StaffColumn.IsBackupOperator, Res.GetString("847DF122-EB0E-4623-B7B4-F1A95DC0816C", "Is Backup Operator"));
				return codeDescriptionPairList;
			}
		}

		protected override BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;
	}
}
