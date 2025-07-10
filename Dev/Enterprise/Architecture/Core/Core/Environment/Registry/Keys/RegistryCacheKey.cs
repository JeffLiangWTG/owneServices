using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryCacheKey
	{
		public RegistryCacheKey(string name, ICompany company, IBranch branch, IDepartment department)
			: this(name, company, branch, department, false)
		{ }

		public RegistryCacheKey(string name, ICompany company, IBranch branch, IDepartment department, bool useFallback)
		{
			Name = name;
			if (company != null)
			{
				CompanyPK = company.PK;
			}
			if (branch != null)
			{
				BranchPK = branch.PK;
			}
			if (department != null)
			{
				DepartmentPK = department.PK;
			}
			UseFallback = useFallback;
		}

		internal RegistryCacheKey(string name, Guid companyPK, Guid branchPK, Guid departmentPK)
			: this(name, companyPK, branchPK, departmentPK, false)
		{ }

		internal RegistryCacheKey(string name, Guid companyPK, Guid branchPK, Guid departmentPK, bool useFallback)
		{
			Name = name;
			CompanyPK = companyPK;
			BranchPK = branchPK;
			DepartmentPK = departmentPK;
			UseFallback = useFallback;
		}

		public Guid OwnerPK
		{
			get { return GetOwnerPK(CompanyPK, BranchPK); }
		}

		public string Key
		{
			get
			{
				if (key == null)
				{
					key = GetKey();
				}
				return key;
			}
		}

		protected virtual string GetKey()
		{
			return Name + GuidsToStringKey(CompanyPK, BranchPK, DepartmentPK) + UseFallback.ToString();
		}

		public static Guid GetOwnerPK(Guid companyPK, Guid branchPK)
		{
			if (companyPK != Guid.Empty && branchPK != Guid.Empty)
			{
				throw new ArgumentException("CompanyPK and BranchPK cannot be specified at the same time. One of them should be Guid.Empty");
			}
			return (companyPK == Guid.Empty) ? branchPK : companyPK;
		}

		internal string GuidsToStringKey(params Guid[] guids)
		{
			char[] r = new char[guids.Length * 8];

			for (int i = 0; i < guids.Length; i++)
			{
				PopulateBytes(r, i * 8, guids[i]);
			}

			return new string(r);
		}

		void PopulateBytes(char[] r, int offset, Guid guid)
		{
			if (guid != Guid.Empty)
			{
				Byte[] m = guid.ToByteArray();

				r[offset + 0] = (char)((m[1] << 8) + m[0]);
				r[offset + 1] = (char)((m[3] << 8) + m[2]);
				r[offset + 2] = (char)((m[5] << 8) + m[4]);
				r[offset + 3] = (char)((m[7] << 8) + m[6]);
				r[offset + 4] = (char)((m[9] << 8) + m[8]);
				r[offset + 5] = (char)((m[11] << 8) + m[10]);
				r[offset + 6] = (char)((m[13] << 8) + m[12]);
				r[offset + 7] = (char)((m[15] << 8) + m[14]);
			}
		}

		string key;
		public readonly string Name;
		public readonly Guid CompanyPK;
		public readonly Guid BranchPK;
		public readonly Guid DepartmentPK;
		public readonly bool UseFallback;
	}
}
