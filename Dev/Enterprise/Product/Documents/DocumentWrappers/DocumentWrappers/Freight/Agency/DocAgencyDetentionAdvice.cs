using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyDetentionAdvice : DocBaseWrapper
	{
		DocAgencyDetentionAdvice(DetentionAdviceHeader advice, BusinessObjectFactory factory)
			: base(advice, factory) { }

		public static DocAgencyDetentionAdvice New(DetentionAdviceHeader advice, BusinessObjectFactory factory)
		{
			return advice == null ? null : new DocAgencyDetentionAdvice(advice, factory);
		}

		public ZDateTime AsAt
		{
			get { return Advice.AsAt; }
		}

		public OrganisationWrapper Client
		{
			get { return client ?? (client = new OrganisationWrapper(OrganisationUsageType.Client, Advice.Client, ContactType.Receivables, Factory)); }
		}
		OrganisationWrapper client;

		public ZBool HasImportContainers
		{
			get { return Advice.Containers.Count > 0; }
		}

		public ZBool HasExportContainers
		{
			get { return Advice.Movements.Count > 0; }
		}

		public DocAgencyDetentionAdviceContainerCollection Overdue
		{
			get
			{
				if (overdue == null)
				{
					ConstructContainerCollections();
				}
				return overdue;
			}
		}
		DocAgencyDetentionAdviceContainerCollection overdue;

		public DocAgencyDetentionAdviceContainerCollection NearDue
		{
			get
			{
				if (nearDue == null)
				{
					ConstructContainerCollections();
				}
				return nearDue;
			}
		}
		DocAgencyDetentionAdviceContainerCollection nearDue;

		public DocAgencyDetentionAdviceContainerCollection NotDue
		{
			get
			{
				if (notDue == null)
				{
					ConstructContainerCollections();
				}
				return notDue;
			}
		}
		DocAgencyDetentionAdviceContainerCollection notDue;

		#region Implementation

		void ConstructContainerCollections()
		{
			notDue = new DocAgencyDetentionAdviceContainerCollection(Factory);
			nearDue = new DocAgencyDetentionAdviceContainerCollection(Factory);
			overdue = new DocAgencyDetentionAdviceContainerCollection(Factory);

			ZDateTime cutOff = Advice.AsAt;
			if (cutOff.IsEmpty)
			{
				throw new InvalidOperationException("AsAt cannot be empty");
			}

			ZDateTime warning = cutOff.AddDays(AgencyRegistry.Instance.DetentionAdviceWarningDays.Value);

			List<DocAgencyDetentionAdviceLine> wrappers = new List<DocAgencyDetentionAdviceLine>();

			foreach (BillOfLadingContainer container in Advice.Containers)
			{
				wrappers.Add(DocAgencyDetentionAdviceLine.New(container, Factory));
			}

			foreach (ContainerMovement movement in Advice.Movements)
			{
				wrappers.Add(DocAgencyDetentionAdviceLine.New(movement, Factory));
			}

			foreach (DocAgencyDetentionAdviceLine wrapper in wrappers)
			{
				if (wrapper.RequiredDate.IsEmpty)
				{
					notDue.Add(wrapper);
				}
				else if (wrapper.RequiredDate < cutOff)
				{
					overdue.Add(wrapper);
				}
				else if (wrapper.RequiredDate < warning)
				{
					nearDue.Add(wrapper);
				}
				else
				{
					notDue.Add(wrapper);
				}
			}
		}

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return Advice.Client; }
		}

		DetentionAdviceHeader Advice
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DetentionAdviceHeader)WrappedObject; }
		}

		#endregion
	}
}
