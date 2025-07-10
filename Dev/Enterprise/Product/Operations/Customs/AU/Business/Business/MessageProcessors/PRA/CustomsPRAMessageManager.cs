using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsPRAMessageManager : IPRAMessageManager
	{
		public CustomsPRAMessageManager(ZString declarationNumber, ZString containerNumber, BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
			JobReference = declarationNumber;
			ContainerNumber = containerNumber;
		}
		readonly BusinessObjectFactory factory;

		public string LastMessageTypeSent
		{
			get
			{
				var result = string.Empty;
				if (Container != null)
				{
					result = (Container.LastPRAMessageSentWasCancellation)
						? PRAConstants.Cancellation
						: PRAConstants.Submission;
				}
				return result;
			}
		}

		public EDIMessageCollection Messages
		{
			get { return Container != null ? Container.Messages : null; }
		}

		public ContainerMessagingData ContainerMessagingData
		{
			get
			{
				return dataLayer ?? (dataLayer = new CustomsDataLayer(Container));
			}
		}
		CustomsDataLayer dataLayer;

		public ZDateTime DepartureDate
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public ZString JobType
		{
			get { return "Declaration No"; }
		}

		public ZString JobReference { get; set; }
		public ZString ContainerNumber { get; set; }

		#region Interface

		protected JobDeclaration Declaration
		{
			get
			{
				return (JobDeclaration)(Container?.Declaration);
			}
		}

		public BusinessObject Parent
		{
			get { return Declaration; }
		}

		BusinessObject IPRAMessageManager.Container
		{
			get { return Container; }
		}

		CusContainer Container
		{
			get
			{
				if (container == null)
				{
					var sQLFilter = new ZDBOnlyQuery(typeof(CusContainer));
					sQLFilter.AddToFilter(CusContainerSchema.CO_ContainerNumber, ContainerNumber);

					var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, JobReference);
					sQLFilter.AddSubQuery(CusContainerSchema.CO_JE, declarationQuery, JoinCondition.And);

					var containers = factory.Load<CusContainer>(sQLFilter);
					if (containers.Length == 1)
					{
						container = containers[0];
					}
				}
				return container;
			}
		}
		CusContainer container;

		#endregion
	}
}
