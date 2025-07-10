using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class OrgHeaderMappingUtils
	{
		public static string GetCompanyCode(OrgHeader organisation)
		{
			string companyCode = string.Empty;
			OrgCodeGenerator newOrgCode = new OrgCodeGenerator();
			companyCode = newOrgCode.GenerateCode(organisation).GetProposedCode();

			if (companyCode.Length == 0)
			{
				companyCode = ManuallyGenerateOrganisationCode(organisation.OH_FullName, organisation.OH_RL_NKClosestPort.SubstringSafe(2, 3));
			}

			companyCode = ValidateOrgCodeIsUnique(organisation, companyCode);

			return companyCode;
		}

		public static string GetCompanyCodeFromExternalCode(OrgHeader organisation, ZString externalCode)
		{
			string companyCode = externalCode;
			if (string.IsNullOrEmpty(companyCode))
			{
				companyCode = ManuallyGenerateOrganisationCode(organisation.OH_FullName, organisation.OH_RL_NKClosestPort.SubstringSafe(2, 3));
			}

			companyCode = ValidateOrgCodeIsUnique(organisation, companyCode);

			return companyCode;
		}

		protected static string ManuallyGenerateOrganisationCode(string companyName, string portCode)
		{
			companyName = companyName.Replace("PTY", "").Replace("LTD", "").Replace("P/L", "").Replace(" AND ", "").Replace("'", "").Replace(".", "").Trim();
			string orgCode = companyName;
			string[] fullNameWords = companyName.Split(" ".ToCharArray());
			char replaceCharacter = '_';
			if (fullNameWords[0].Length < 3)
			{
				fullNameWords[0] = fullNameWords[0].PadRight(3, replaceCharacter);
			}

			if (fullNameWords.Length > 1)
			{
				if (fullNameWords[1].Length < 3)
				{
					fullNameWords[1] = fullNameWords[1].PadRight(3, replaceCharacter);
				}
				string cNPart1 = fullNameWords[0].Substring(0, 3);
				string cNPart2 = fullNameWords[1].Substring(0, 3);
				orgCode = cNPart1 + cNPart2 + portCode;
			}
			else if (fullNameWords.Length > 0)
			{
				string cNPart1 = "";
				if (fullNameWords[0].Length > 5)
				{
					cNPart1 = fullNameWords[0].Substring(0, 6);
				}
				else
				{
					cNPart1 = companyName;
				}
				orgCode = cNPart1 + portCode;
			}

			return orgCode;
		}

		protected static string ValidateOrgCodeIsUnique(OrgHeader organisation, ZString orgCode)
		{
			orgCode = orgCode.Left(OrgHeader.Schema.OH_CodeMaxLength);
			bool uniqueCode = false;
			int uniquenessAdjustment = 0;

			while (!uniqueCode && orgCode.Length <= OrgHeader.Schema.OH_CodeMaxLength)
			{
				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, orgCode);
				filter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, organisation.PK);
				OrgHeader sameCodeOrganisation = (OrgHeader)organisation.Factory.LoadTop1(typeof(OrgHeader), filter);

				if (sameCodeOrganisation == null)
				{
					uniqueCode = true;
				}
				else
				{
					uniquenessAdjustment++;
					orgCode = orgCode.Left(9) + uniquenessAdjustment.ToString();
				}
			}

			if (!uniqueCode)
			{
				throw new Exception("Unable to generate a unique code for the Organization [" + orgCode + "]");
			}

			return orgCode.ToString();
		}
	}
}
