using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class FsdWizard : AutoFsdWizard
	{
		public FsdWizard(BusinessObjectFactory factory)
			: base(factory) { }

		public override ZInt NumberSDIActual
		{
			get { return base.NumberSDIActual; }
			set
			{
				base.NumberSDIActual = value;
				BuildAiStatement();
			}
		}
		public override ZInt NumberSDWActual
		{
			get { return base.NumberSDWActual; }
			set
			{
				base.NumberSDWActual = value;
				BuildAiStatement();
			}
		}

		public override ZInt NumberSDIExpected
		{
			get { return base.NumberSDIExpected; }
			set
			{
				base.NumberSDIExpected = value;
				BuildAiStatement();
			}
		}
		public override ZInt NumberSDWExpected
		{
			get { return base.NumberSDWExpected; }
			set
			{
				base.NumberSDWExpected = value;
				BuildAiStatement();
			}
		}

		public void BuildAiStatement()
		{
			AiStatementInfo.SetValueFromString(string.Format("SDI = {0}/{1}, SDW = {2}/{3}", NumberSDIActual, NumberSDIExpected, NumberSDWActual, NumberSDWExpected));
			AiStatementInfo.RefreshBinding();
		}

		[List(nameof(Consignees))]
		public override ZGuid Consignee
		{
			get { return base.Consignee; }
			set { base.Consignee = value; }
		}

		[List(nameof(Consignors))]
		public override ZGuid Consignor
		{
			get { return base.Consignor; }
			set { base.Consignor = value; }
		}

		[List(nameof(Authorizations))]
		public override ZGuid Authorization
		{
			get { return base.Authorization; }
			set { base.Authorization = value; }
		}

		[List(nameof(EidrTypeList))]
		public override ZString Procedure
		{
			get { return base.Procedure; }
			set { base.Procedure = value; }
		}

		public OrgHeaderCollection Consignors
		{
			get
			{
				return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			}
		}

		public OrgHeaderCollection Consignees
		{
			get
			{
				return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			}
		}

		public CusAuthorisationHeaderCollection Authorizations
		{
			get
			{
				var result = new CusAuthorisationHeaderCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", new ZString(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration)));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", Consignee));
				return result;
			}
		}

		public CusAuthorisationHeader AuthorizationHeader => Factory.Load<CusAuthorisationHeader>(Authorization);

		public CodeDescriptionPairList EidrTypeList
		{
			get { return Factory.GetCachedValue<EidrTypeList>(); }
		}
	}
}
