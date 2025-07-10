using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	class AllocationInfo
	{
#if DEBUG
		public
#else
		internal
#endif
		AllocationInfo(ZString rawAllocationInfo)
		{
			SetDefaults();
			var allocationInfoToProcess = GetAllocationInfoToProcess(rawAllocationInfo);
			Process(allocationInfoToProcess);
		}

#if DEBUG
		public
#else
		internal
#endif
		ZString RecordCompanyCode { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString VisibleCompanyCode { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString VisibleBranchCode { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString VisibleDepartmentCode { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString RefType { get; set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString DocType { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString RefCode { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		ZString DocSource { get; private set; }

		void SetDefaults()
		{
			RecordCompanyCode = ZString.Empty;
			VisibleBranchCode = ZString.Empty;
			VisibleCompanyCode = ZString.Empty;
			VisibleDepartmentCode = ZString.Empty;
			RefType = ZString.Empty;
			DocType = ZString.Empty;
			RefCode = ZString.Empty;
			DocSource = ZString.Empty;
		}

		ZString GetAllocationInfoToProcess(ZString rawAllocationInfo)
		{
			int startIndex = rawAllocationInfo.IndexOf('[');
			int endIndex = startIndex >= 0 ? rawAllocationInfo.IndexOf("]", startIndex, StringComparison.InvariantCulture) : -1;
			if (startIndex >= 0 && endIndex > startIndex)
			{
				return rawAllocationInfo.SubstringSafe(startIndex, endIndex - startIndex + 1);
			}
			else
			{
				return rawAllocationInfo;
			}
		}

		void Process(ZString allocationInfo)
		{
			allocationInfo = allocationInfo.Trim(']', '[', ' ');
			var componentsString = allocationInfo;
			Regex regex = new Regex(@" [BCDS](?<Seperator>:|@).{3}");
			var match = regex.Match(allocationInfo);
			if (match.Success)
			{
				var companyCodeSeperator = match.Groups["Seperator"].Value[0];
				componentsString = allocationInfo.Substring(0, match.Index);
				foreach (Match m in regex.Matches(allocationInfo))
				{
					var parts = m.Value.Trim().Split(companyCodeSeperator);
					switch (parts[0])
					{
						case "B":
							VisibleBranchCode = parts[1];
							break;
						case "C":
							VisibleCompanyCode = parts[1];
							RecordCompanyCode = parts[1];
							break;
						case "D":
							VisibleDepartmentCode = parts[1];
							break;
						case "S":
							DocSource = parts[1];
							break;
					}
				}
			}

			var components = componentsString.Trim().Split(' ');
			var numberOfComponents = components.Length;

			if (numberOfComponents > 0)
			{
				int index = components[0].Contains("DocManager", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

				Func<ZString> getNextComponent = () => (numberOfComponents > index) ? components[index++] : ZString.Empty;

				RefType = getNextComponent().ToUpper();

				DocType = getNextComponent();
				RefCode = getNextComponent();

				while (index < numberOfComponents)
				{
					RefCode += " " + getNextComponent();
				}
			}
		}

		internal void SetVisibleCompanyPK(VisibleCompanyBranchDepartmentInfo visibleInfo, DocumentFactory masterFactory, ref string errorMessage)
		{
			if (!string.IsNullOrEmpty(VisibleCompanyCode))
			{
				var visibleCompany = masterFactory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, VisibleCompanyCode);
				if (visibleCompany != null)
				{
					visibleInfo.VisibleCompanyPK = visibleCompany.PK;
				}
				else
				{
					if (!string.IsNullOrEmpty(errorMessage))
					{
						errorMessage += System.Environment.NewLine;
					}
					errorMessage += Res.GetString("3b1f8f5d-f8b6-4ac5-9909-de4251630b31", "The visible company code '{0}' specified in email subject is invalid.", VisibleCompanyCode);
				}
			}
		}

		internal void SetVisibleBranchPK(VisibleCompanyBranchDepartmentInfo visibleInfo, DocumentFactory masterFactory, ref string errorMessage)
		{
			if (!string.IsNullOrEmpty(VisibleBranchCode))
			{
				var visibleBranch = masterFactory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, VisibleBranchCode);
				if (visibleBranch != null)
				{
					visibleInfo.VisibleBranchPK = visibleBranch.PK;

					if (visibleInfo.VisibleCompanyPK.IsEmpty)
					{
						visibleInfo.VisibleCompanyPK = visibleBranch.Company.PK;
					}
					else if (visibleInfo.VisibleCompanyPK != visibleBranch.Company.PK)
					{
						if (!string.IsNullOrEmpty(errorMessage))
						{
							errorMessage += System.Environment.NewLine;
						}
						errorMessage += Res.GetString("9115AEAE-2F06-47FE-B30B-67206BCF5D3B", "The visible branch code '{0}' does not belong to the visible company specified in email subject.", VisibleBranchCode);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(errorMessage))
					{
						errorMessage += System.Environment.NewLine;
					}
					errorMessage += Res.GetString("40D0BEE7-C8E6-4DFD-8E13-39EDAFF061FB", "The visible branch code '{0}' specified in email subject is invalid.", VisibleBranchCode);
				}
			}
		}

		internal void SetVisibleDepartmentPK(VisibleCompanyBranchDepartmentInfo visibleInfo, DocumentFactory masterFactory, ref string errorMessage)
		{
			if (!string.IsNullOrEmpty(VisibleDepartmentCode))
			{
				var visibleDepartment = masterFactory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, VisibleDepartmentCode);
				if (visibleDepartment != null)
				{
					visibleInfo.VisibleDepartmentPK = visibleDepartment.PK;
				}
				else
				{
					if (!string.IsNullOrEmpty(errorMessage))
					{
						errorMessage += System.Environment.NewLine;
					}
					errorMessage += Res.GetString("8F12516E-8C7E-4A2D-BF98-DA1015A1C9CE", "The visible department code '{0}' specified in email subject is invalid.", VisibleDepartmentCode);
				}
			}
		}
	}
#if DEBUG
	public
#else
	internal
#endif
	class VisibleCompanyBranchDepartmentInfo
	{
		public ZGuid VisibleCompanyPK { get; set; }

		public ZGuid VisibleBranchPK { get; set; }

		public ZGuid VisibleDepartmentPK { get; set; }

		public bool IsEmpty()
		{
			return VisibleCompanyPK.IsEmpty && VisibleBranchPK.IsEmpty && VisibleDepartmentPK.IsEmpty;
		}
	}
}
