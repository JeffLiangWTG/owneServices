using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusPermitHeader : BaseCusPermitHeader, IDISHostProvider, ICADIFHost, ICusPermitHeader
	{
		public CusPermitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		IDISHost IDISHostProvider.DISHost => this;

		#region IDISHost Members

		BusinessObjectFactory IDISHost.Factory => Factory;

		ZGuid IDISHost.PK => PK;

		ZBool IDISHost.ShowDISFeatures => CPH_RN_NKCountryCode == Core.Constants.CountryCodes.Canada;

		ZGuid IDISHost.BranchPK
		{
			get
			{
				var company = Factory.Load<GlbCompany>(((IDISHost)this).CompanyPK);
				return company?.FirstActiveBranch?.PK ?? ZGuid.Empty;
			}
		}

		ZGuid IDISHost.CompanyPK
		{
			get
			{
				foreach (GlbCompany company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Canada, Factory))
				{
					if (!string.IsNullOrEmpty(CACustomsDataRegistry.Instance.AccountSecurityNo.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)))
					{
						return company.PK;
					}
				}
				return ZGuid.Empty;
			}
		}

		IEnumerable<string> IDISHost.ApplicationCodes => new[] { Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF };

		IHaveRequiredDocuments IDISHost.RequiredDocumentsProvider => this;

		IEnumerable<IeDoc> IDISHost.EDocs
		{
			get { return DocManagerInfo.GetRelatedEDocsView(); }
		}

		ZString IDISHost.JobNumber => CPH_Number;

		ZString IDISHost.HumanReadable => "DIF";

		ZBool IDISHost.DISEditable => Environment.Env.Security.CACustomsDIFEdit.IsAllowed;

		IControllerIDProvider IDISHost.ControllerIDProvider => this;

		ZString IDISHost.ImporterName => ZString.Empty;

		IEnumerable<ZString> IDISHost.ErrorMessages => Array.Empty<ZString>();

		Integration.Customs.Shared.IDISReferenceNumberFountainStrategy IDISHost.DISReferenceNumberFountainStrategy => new DIFReferenceNumberFountainStrategy(Factory);

		event EventHandler IDISHost.DISFeatureVisibilityChanged
		{
			add { }
			remove { }
		}

		bool IDISHost.DoPreFormAction() => true;

		bool IDISHost.NeedToDoPreFormAction() => false;

		#region ICADIFHost

		ICADIFDefaultValues ICADIFHost.ValueProvider => new DIFDefaultValuesCusPermitHeaderWrapper(this);

		ZBool ICADIFHost.ShouldSendChangeMessageAsAmendment => false;

		ZString ICADIFHost.ImporterBusinessNumber => PermitHolder?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada)?.OK_CustomsRegNo ?? ZString.Empty;

		OrgHeader ICADIFHost.BusinessNumberHolder => PermitHolder;

		ZDateTime ICADIFHost.DateOfArrival => ZDateTime.Empty;

		#endregion

		#endregion

		#region JobRequiredDocumentAddInfo

		public ZString DIFURNs
		{
			get
			{
				var addinfos = JobRequiredDocumentAddInfos;
				var count = addinfos.Take(2).Count();
				if (count > 1)
				{
					return MultipleValues;
				}
				else if (count == 1)
				{
					return addinfos.FirstOrDefault().EX_ReferenceNumber;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString DIFMessageStatus
		{
			get
			{
				var addinfos = JobRequiredDocumentAddInfos.DistinctBy(addinfo => addinfo.EX_Status).ToArray();
				var count = addinfos.Length;
				if (count > 1)
				{
					return MultipleValues;
				}
				else if (count == 1)
				{
					return Factory.GetCachedValue<Common.CA.DIF.StatusList>().GetDescriptionFromCode(addinfos[0].EX_Status);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		IEnumerable<JobRequiredDocumentAddInfo> JobRequiredDocumentAddInfos => RequiredDocuments.Cast<JobRequiredDocument>()
					.SelectMany(document => document.AddInfos.Cast<JobRequiredDocumentAddInfo>())
					.Where(addinfo =>
					{
						return addinfo.EX_ApplicationCode == Constants.Customs.DocumentImageSystemIDs.CA_DIF
							&& addinfo.EX_GC_Company == GlbCompany.CurrentCompany.PK;
					});

		static readonly MultilingualString MultipleValues = ResString.GetMultilingualString("0DFEC00D-793C-44EE-8052-99EF8AF8A491", "Multiple");

		#endregion
	}
}
