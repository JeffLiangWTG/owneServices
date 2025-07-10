namespace Enterprise.ZArchitecture.Core
{
	public class CreateMISCOrgScripts : CreateSystemOrgScripts
	{
		#region SuppressResourceStringsCheckRegion

		protected internal override string OrgCode
		{
			get { return "MISC"; }
		}

		protected internal override string OrgName
		{
			get { return "MISCELLANEOUS ORGANISATION (SYSTEM DEFINED)"; }
		}

		protected internal override string OS_FullCompanyName
		{
			get { return "MISCELLANEOUS ORGANISATION SYSTEM DEFINED"; }
		}

		protected internal override string OS_CompanyName1
		{
			get { return "M245"; }
		}

		protected internal override string OS_CompanyName2
		{
			get { return "O625"; }
		}

		protected internal override string OS_CompanyName3
		{
			get { return "S235"; }
		}

		protected internal override string OS_CompanyName4
		{
			get { return "D153"; }
		}

		protected internal override string OS_Address1
		{
			get { return "A362"; }
		}

		protected internal override string OS_Address2
		{
			get { return "S121"; }
		}

		protected internal override string OS_Address3
		{
			get { return "S235"; }
		}

		protected internal override string OS_Address4
		{
			get { return "D153"; }
		}

		protected internal override string OS_City
		{
			get { return "N000"; }
		}

		protected internal override string OS_State
		{
			get { return "N200"; }
		}

		#endregion
	}
}
