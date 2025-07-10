using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business
{
	public class JASOrgHeader : OrgHeader
	{
		#region Static

		public static JASOrgHeader FindOrgHeaderByOfficeAndNettingCode(BusinessObjectFactory factory, ZString officeCode, ZString nettingCode)
		{
			ZQuery officeCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.UniversalOfficeCode);
			officeCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, officeCode);
			OrgCusCode[] officeCodes = (OrgCusCode[])factory.Load(typeof(OrgCusCode), officeCodeFilter);

			ZQuery nettingCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.UniversalNettingCode);
			nettingCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, nettingCode);
			OrgCusCode[] nettingCodes = (OrgCusCode[])factory.Load(typeof(OrgCusCode), nettingCodeFilter);

			foreach (OrgCusCode nettingOrgCusCode in nettingCodes)
			{
				foreach (OrgCusCode officeOrgCusCode in officeCodes)
				{
					if (nettingOrgCusCode.OK_OH == officeOrgCusCode.OK_OH)
					{
						return (JASOrgHeader)nettingOrgCusCode.Header;
					}
				}
			}

			return null;
		}

		public static JASOrgHeader FindOrgHeaderByOfficeCode(BusinessObjectFactory factory, ZString officeCode)
		{
			ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.UniversalOfficeCode);
			filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, officeCode);
			OrgCusCode officeOrgCusCode = (OrgCusCode)factory.LoadTop1(typeof(OrgCusCode), filter);
			return (officeOrgCusCode != null) ? (JASOrgHeader)officeOrgCusCode.Header : null;
		}

		public static JASOrgHeader FindOrgHeaderByJASWWMappedCode(BusinessObjectFactory factory, ZString jASWWMappedCode)
		{
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, jASWWMappedCode);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, JASDataRegistry.Instance.JASWWOrganisationPK);
			OrgPatternMatchOverride patternMatchOverride = (OrgPatternMatchOverride)factory.LoadTop1(typeof(OrgPatternMatchOverride), filter);
			return (patternMatchOverride != null) ? (JASOrgHeader)patternMatchOverride.Header : null;
		}

		#endregion

		public JASOrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZBool IsJASOffice
		{
			get { return !OfficeCode.IsEmpty && !NettingCode.IsEmpty; }
		}

		public ZString OfficeCode
		{
			get { return CustomsCodes.GetUOC(); }
			set
			{
				OrgCusCode officeCodeObject = LoadOrCreateOrgCusCode(OrgCusCode.CodeTypes.UniversalOfficeCode);
				officeCodeObject.OK_CustomsRegNo = value;
			}
		}

		public ZString NettingCode
		{
			get { return CustomsCodes.GetUNC(); }
			set
			{
				OrgCusCode nettingCodeObject = LoadOrCreateOrgCusCode(OrgCusCode.CodeTypes.UniversalNettingCode);
				nettingCodeObject.OK_CustomsRegNo = value;
			}
		}

		public ZString JASWWMappedCode
		{
			get
			{
				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, JASDataRegistry.Instance.JASWWOrganisationPK);
				filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, PK);

				return Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_ForeignCode ?? ZString.Empty;
			}
		}

		OrgCusCode LoadOrCreateOrgCusCode(ZString codeType)
		{
			OrgCusCode result = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, (RefCountry)null);
			if (result == null)
			{
				result = CustomsCodes.AddNew();
				result.OK_CodeType = codeType;
			}
			return result;
		}
	}
}

#region Implementation
#endregion
