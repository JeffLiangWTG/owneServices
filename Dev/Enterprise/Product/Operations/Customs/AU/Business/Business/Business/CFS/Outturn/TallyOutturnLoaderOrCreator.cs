using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal class TallyOutturnLoaderOrCreator
	{
		public TallyOutturnLoaderOrCreator(TallyContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			this.container = container;
		}

		#region Finding

		public TallyOutturn FindOutturn()
		{
			TallyOutturn result = null;
			TallyOutturnHeader header = FindHeader();

			if (header != null)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.FullContainerLoad);
				query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, container.JC_ContainerNum);
				BusinessObject[] outturns = header.Outturns.Find(query);

				if (outturns.Length > 1)
				{
					throw new FoundTooManyMatchesException();
				}

				if (outturns.Length == 1)
				{
					result = (TallyOutturn)outturns[0];
					if (result.Parent != null)
					{
						throw new AlreadyLinkedException();
					}
				}
			}

			return result;
		}

		internal ZString CurrentBranchPremiseID
		{
			get
			{
				if (!currentBranchPremiseID.HasValue)
				{
					if (GlbBranch.CurrentBranch.OrgProxy != null)
					{
						currentBranchPremiseID = GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID;
					}

					if (!currentBranchPremiseID.HasValue || currentBranchPremiseID.Value == ZString.Empty)
					{
						currentBranchPremiseID = GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
					}
				}
				return currentBranchPremiseID.Value;
			}
		}
		ZString? currentBranchPremiseID;

#if DEBUG
		internal void ClearCachedValues()
		{
			currentBranchPremiseID = null;
		}
#endif

		public TallyOutturnHeader FindHeader()
		{
			TallyOutturnHeader result = null;
			CFSLoadListConsol consol = container.Consol
				?? throw new ContainerHasNoConsolException();

			ZQuery query = new ZQuery();
			query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, consol.MainTransport.JW_VoyageFlight);
			query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, CurrentBranchPremiseID);
			query.AddToFilter(CusOutturnHeaderSchema.C6_VesselName, consol.MainTransport.JW_Vessel);

			TallyOutturnHeader[] foundHeaders = factory.Load<TallyOutturnHeader>(query);

			if (foundHeaders != null && foundHeaders.Length == 0)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, consol.MainTransport.JW_VoyageFlight);
				query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, CurrentBranchPremiseID);
				query.AddToFilter(CusOutturnHeaderSchema.C6_VesselName, consol.MainTransport.JW_Vessel);

				foundHeaders = factory.Load<TallyOutturnHeader>(query);
			}

			if (foundHeaders.Length > 1)
			{
				throw new FoundTooManyMatchesException();
			}

			if (foundHeaders.Length == 1)
			{
				result = foundHeaders[0];
			}

			return result;
		}

		#endregion

		#region Creating

		public TallyOutturn CreateOutturnAndHeader()
		{
			CFSLoadListConsol consol = container.Consol
				?? throw new ContainerHasNoConsolException();

			TallyOutturnHeader header = factory.New<TallyOutturnHeader>();
			header.C6_OutturningPremiseID = CurrentBranchPremiseID;
			header.C6_VoyageNum = consol.MainTransport.JW_VoyageFlight;
			header.C6_VesselName = consol.MainTransport.JW_Vessel;
			return CreateOutturn(header);
		}

		public TallyOutturn CreateOutturn(TallyOutturnHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException(nameof(header));
			}

			TallyOutturn result = header.Outturns.AddNew();
			result.C5_ContainerNumber = container.JC_ContainerNum;
			result.C5_CargoType = MapToCustomsContainerMode(container.JC_ContainerMode);
			return result;
		}

		#endregion

		#region Exceptions

		[Serializable]
		public class FoundTooManyMatchesException : ApplicationException
		{
			public FoundTooManyMatchesException()
				: base()
			{ }

#if NETFRAMEWORK
			protected FoundTooManyMatchesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		[Serializable]
		public class ContainerHasNoConsolException : ApplicationException
		{
			public ContainerHasNoConsolException()
				: base()
			{ }

#if NETFRAMEWORK
			protected ContainerHasNoConsolException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		[Serializable]
		public class AlreadyLinkedException : ApplicationException
		{
			public AlreadyLinkedException()
				: base()
			{ }

#if NETFRAMEWORK
			protected AlreadyLinkedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		#endregion

		#region Implementation

		ZString MapToCustomsContainerMode(ZString containerMode)
		{
			return CMRImportCargoTypes.Codes.FullContainerLoad;
		}

		readonly TallyContainer container;

		BusinessObjectFactory factory
		{
			get { return container.Factory; }
		}

		#endregion
	}
}
