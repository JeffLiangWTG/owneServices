using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusMAWBDocumentSupporter : DocumentSupporter
	{
		public CTOCusMAWBDocumentSupporter(CTOCusMAWB parent)
			: base(parent)
		{
		}

		protected CTOCusMAWB CTOCusMAWB
		{
			get { return (CTOCusMAWB)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CTOCusMAWB; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun.SU_MenuName.Equals("Flight Manifest"))
			{
				try
				{
					ZDecimal weight;
					foreach (CTOCusHAWB hawb in CTOCusMAWB.ChildBills)
					{
						weight = Core.Constants.Weight.Convert(hawb.CS_Weight, hawb.CS_WeightUQ, Env.Registry.FreightWeightUnit);
					}
				}
				catch (System.ArgumentException)
				{
					// conversion failed, some of the unit has to be illegal.
					// see Issue 00928720
					return new DocumentSupporterDataState(false, "There is one of more invalid weight unit(s)");
				}
			}
			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, CTOCusMAWB);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			switch (dataContext)
			{
				case Core.Constants.DataContext.CTOCusMAWB:
					return new DocumentWrapper[]
					{
						DocumentWrapperFactory.CreateCustomsWrapper(dataContext, Parent, "AU")
					};

				default:
					return System.Array.Empty<DocumentWrapper>();
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.CTOCusMAWB, Core.Constants.DataContext.GenericFreightJob };
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Core.Constants.DataContext.CTOCusMAWB)
			{
				return Res.GetString("D84DA597-B8CC-462B-ADF2-4917869F7AAA", "This shipment is not associated with a MAWB data.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.GenericFreightJob && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Implementation

		CTOCusMAWB Parent
		{
			get { return (CTOCusMAWB)BusinessObject; }
		}

		#endregion
	}
}
