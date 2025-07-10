using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OrganisationCPQA : NonPersistentBusinessObject, ICPQALineAttachee, IObsoleteValidation
	{
		public OrganisationCPQA(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.Organisation = organisation;
		}

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		#region ICPQAAttachee members

		ZGuid ICPQAAttachee.PK
		{
			get { return Organisation.PK; }
		}

		public SchemaGuidColumn FKColumnInCusEntryCPDecTable
		{
			get { return CusEntryCPDecSchema.ON_ParentID; }
		}

		public CMRCusEntryCPDecCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new CMROrgCusEntryCPDecCollection(this);
					fQuestions.Load();
					Organisation.RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}
		CMRCusEntryCPDecCollection fQuestions;

		public ZDateTime SelectionDate
		{
			get { return ZDateTime.Today; }
		}

		#endregion

		#region ICPQALineAttachee members

		ICPQALineAttachee[] ICPQALineAttachee.SourcesToDefault
		{
			get { return System.Array.Empty<ICPQALineAttachee>(); }
		}

		LineDefaultQuestions ICPQALineAttachee.DefaultUniqueQuestions
		{
			get { return new LineDefaultQuestions(); }
		}

		CPQuestionKeys ICPQALineAttachee.CPQuestionKey
		{
			get { return new CPQuestionKeys(); }
		}

		ZString ICPQALineAttachee.TableCode
		{
			get { return OrgHeaderSchema.Constants.Prefix; }
		}

		ZBool ICPQALineAttachee.IsRiskCalculatedFromTariff
		{
			get { return false; }
		}

		ZBool ICPQALineAttachee.IsRiskHistorySupported
		{
			get { return false; }
		}

		#endregion

		public override bool IsInDatabase
		{
			get { return Organisation.IsInDatabase; }
		}

		public readonly OrgHeader Organisation;
	}
}
