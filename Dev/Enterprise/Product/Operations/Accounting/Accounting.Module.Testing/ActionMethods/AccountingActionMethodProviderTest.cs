using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccountingActionMethodProvider))]
	public class AccountingActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			OperationalActionMethod autoRateCostsRevenueActionMethod = new AutoRateCostsRevenueActionMethod(new JobInvoicingSecurityHelper(Env.Security.None));
			OperationalActionMethod autoRateRevenueActionMethod = new AutoRateRevenueActionMethod(new JobInvoicingSecurityHelper(Env.Security.None));
			OperationalActionMethod autoRateCostsActionMethod = new AutoRateCostsActionMethod(new JobInvoicingSecurityHelper(Env.Security.None));
			OperationalActionMethod exchangeRatesMethod = new ExchangeRatesActionMethod(null);
			OperationalActionMethod profitShareMethod = new ProfitShareActionMethod(typeof(OperationalActionSupporter));
			OperationalActionMethod calculatePivotsAndRateMethod = new ConsolEqualizeAndAutorateActionMethod(new JobInvoicingSecurityHelper(Env.Security.None));
			OperationalActionMethod updateJobStatusMethod = new UpdateJobStatusActionMethod();
			OperationalActionMethod updateJobDeptActionMethod = new UpdateJobDeptActionMethod();
			OperationalActionMethod updateJobLocalClientActionMethod = new UpdateJobLocalClientActionMethod();
			OperationalActionMethod updateJobOperatorActionMethod = new UpdateJobOperatorActionMethod();
			OperationalActionMethod updateJobOverseasAgentActionMethod = new UpdateJobOverseasAgentActionMethod();
			OperationalActionMethod updateJobProfitLossReasonActionMethod = new UpdateJobProfitLossReasonActionMethod();

			AssertNull(Provider.NewMethods(new DummyActionSupporter<DummyBusinessObject>()));

			OperationalActionMethod[] updateJobMethods = new OperationalActionMethod[]
			{
				updateJobStatusMethod,
				updateJobDeptActionMethod,
				updateJobLocalClientActionMethod,
				updateJobOperatorActionMethod,
				updateJobOverseasAgentActionMethod,
				updateJobProfitLossReasonActionMethod
			};

			OperationalActionMethod[] expectedMethods1 = new OperationalActionMethod[] { exchangeRatesMethod, profitShareMethod };
			var expectedMethods = updateJobMethods.Union(expectedMethods1);

			OperationalActionMethod[] methods = Provider.NewMethods(new DummyActionSupporter<DummyAutoRating>());
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", Array.Empty<ExRateSourceType>(), methods);

			AssertNull(Provider.NewMethods(new DummyActionSupporterWithInvoicingCheckpoint<DummyBusinessObject>(Env.Security.None)));

			methods = Provider.NewMethods(new DummyActionSupporterWithInvoicingCheckpoint<DummyAutoRating>(null));
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", Array.Empty<ExRateSourceType>(), methods);

			expectedMethods1 = new[] { autoRateCostsRevenueActionMethod, autoRateRevenueActionMethod, autoRateCostsActionMethod, exchangeRatesMethod, profitShareMethod, calculatePivotsAndRateMethod };
			expectedMethods = updateJobMethods.Union(expectedMethods1);
			methods = Provider.NewMethods(new DummyActionSupporterWithInvoicingCheckpoint<DummyAutoRating>(Env.Security.None));
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", Array.Empty<ExRateSourceType>(), methods);

			expectedMethods1 = new[] { autoRateCostsRevenueActionMethod, autoRateRevenueActionMethod, autoRateCostsActionMethod, exchangeRatesMethod, profitShareMethod, calculatePivotsAndRateMethod };
			expectedMethods = updateJobMethods.Union(expectedMethods1);
			methods = Provider.NewMethods(new DummyActionSupporterWithInvoicingCheckpoint<DummyAutoRatingWithInvoicingExRateSourceProvider>(Env.Security.None));
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", new ExRateSourceType[] { ExRateSourceType.Voyage }, methods);

			expectedMethods1 = new[] { exchangeRatesMethod, profitShareMethod };
			expectedMethods = updateJobMethods.Union(expectedMethods1);
			methods = Provider.NewMethods(new DummyActionSupporter<DummyBusinessObjectWithInvoicingPlugIn>());
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", Array.Empty<ExRateSourceType>(), methods);

			expectedMethods1 = new[] { exchangeRatesMethod, profitShareMethod };
			expectedMethods = updateJobMethods.Union(expectedMethods1);
			methods = Provider.NewMethods(new DummyActionSupporter<DummyExRatesSource>());
			AssertNotNull("should have returned a methods array", methods);
			AssertContainsExactElementsInAnyOrder("Assert correct methods returned", new ActionMethodEqualityComparer(), expectedMethods, methods);
			AssertSupportedExchangeRateSources("supported ex rate sources", new ExRateSourceType[] { ExRateSourceType.Voyage }, methods);
		}

		#region Implementation

		void AssertSupportedExchangeRateSources(string message, ExRateSourceType[] expectedSourceTypes, IEnumerable<OperationalActionMethod> methods)
		{
			foreach (OperationalActionMethod method in methods)
			{
				ExchangeRatesActionMethod exRateMethod = method as ExchangeRatesActionMethod;

				if (exRateMethod != null)
				{
					AssertContainsExactElementsInAnyOrder(message, expectedSourceTypes, exRateMethod.RateSources ?? Array.Empty<ExRateSourceType>());
				}
			}
		}

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.Accounting; }
		}

		#endregion

		#region HelperClasses

		#region NameAndTypeEqualityComparer

		class ActionMethodEqualityComparer : IEqualityComparer<OperationalActionMethod>
		{
			#region IEqualityComparer<OperationalActionMethod> Members

			public bool Equals(OperationalActionMethod x, OperationalActionMethod y)
			{
				if (x == null && y == null)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				if (x.Name != y.Name)
				{
					return false;
				}

				return true;
			}

			public int GetHashCode(OperationalActionMethod obj)
			{
				return obj == null ? 0 : (obj.GetType().GetHashCode() ^ obj.Name.GetHashCode());
			}

			#endregion
		}

		#endregion

		#region DummyActionSupporter

		class DummyActionSupporter<BusinessObjectT> : OperationalActionSupporter
		{
			public override BusinessContext BusinessContext
			{
				get { return (BusinessContext)(-1); }
			}

			public override Type RootType
			{
				get { return typeof(BusinessObjectT); }
			}
		}

		#endregion

		#region DummyActionSupporterWithInvoicingCheckpoint

		class DummyActionSupporterWithInvoicingCheckpoint<BusinessObjectT> : DummyActionSupporter<BusinessObjectT>, IInvoicingSecurityCheckpointProvider
		{
			public DummyActionSupporterWithInvoicingCheckpoint(SecurityCheckpoint checkpoint)
			{
				this.checkpoint = checkpoint;
			}

			#region IInvoicingSecurityCheckpointProvider Members

			public SecurityCheckpoint InvoicingCheckpoint
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return checkpoint; }
			}

			#endregion

			readonly SecurityCheckpoint checkpoint;
		}

		#endregion

		#region DummyAutoRating

		class DummyAutoRating : DummyBusinessObject, IRatingSupporter, IJobInvoicingPlugIn, IStmNoteParent
		{
			public DummyAutoRating(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			#region IJobInvoicingPlugIn Members

			public IJobInvoicingSupporter InvoicingSupporter
			{
				get { return invoicingSupporter ?? (invoicingSupporter = new JobInvoicingSupporter(this)); }
			}

			JobInvoicingSupporter invoicingSupporter;

			#endregion

			#region IJobHeaderParent Members

			public void OnJobCreating(JobHeader job)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public void OnJobCreated(JobHeader job)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public void OnJobDeleting(JobHeader job)
			{
			}

			public void OnJobDeleted(JobHeader job)
			{
			}

			public void SetJobNumberFieldOnSaving()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			#endregion

			#region IJobNumber Members

			public string JobNumber
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			#endregion

			#region IStmNoteParent Members

			Notes IStmNoteParent.Notes
			{
				get { return notes ?? (notes = new Notes(this)); }
			}
			Notes notes;

			ZGuid IStmNoteParent.NotesParentPK
			{
				get { return PK; }
			}

			string IStmNoteParent.NotesParentTableName
			{
				get { return TableName; }
			}

			BusinessObjectFactory IStmNoteParent.NotesFactory
			{
				get { return Factory; }
			}

			GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate { get; set; }

			bool IStmNoteParent.SupportsNotes
			{
				get { return true; }
			}

			BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
			{
				get { return Array.Empty<BusinessObject>(); }
			}

			StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
			{
				get { return StmNoteContexts.Default; }
			}

			NoteTypeCollection IStmNoteParent.NoteTypes
			{
				get
				{
					var types = new NoteTypeCollection();
					types.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
					return types;
				}
			}

			#endregion

			#region IRatingSupporter Members

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get { return new DummyAutoRatingAdaptersProvider(this); }
			}

			#endregion
		}

		#endregion

		#region DummyAutoRatingAdaptersProvider

		class DummyAutoRatingAdaptersProvider : RatingAdaptersProvider<DummyAutoRating>
		{
			public DummyAutoRatingAdaptersProvider(DummyAutoRating parent) : base(parent)
			{
			}

			protected override List<IAutoRating> GetAdapters(DummyAutoRating parent, IAutoRatingInteractor uiInteractor, Enterprise.Integration.Accounting.AutoRateOptions options)
			{
				return new List<IAutoRating> { new DummyAutoRatingAdapter<DummyAutoRating>(parent) };
			}
		}

		#endregion

		#region DummyAutoRatingAdapter

		class DummyAutoRatingAdapter<T> : RatingAdapter<T>
			where T : DummyAutoRating
		{
			public DummyAutoRatingAdapter(T parent)
				: base(parent)
			{
			}

			#region RatingAdapter

			public override IJobInvoicingSupporter InvoicingSupporter
			{
				get { return Parent.InvoicingSupporter; }
			}

			#endregion
		}

		#endregion

		#region DummyExRatesSource

		[SupportExRateSource(ExRateSourceType.Voyage)]
		class DummyAutoRatingWithInvoicingExRateSourceProvider : DummyAutoRating, IJobInvoicingExRateSourceProvider
		{
			public DummyAutoRatingWithInvoicingExRateSourceProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IJobInvoicingExRateSourceProvider Members

			IExchangeRateSource IJobInvoicingExRateSourceProvider.GetExRateSource(ExRateSourceType sourceType)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		class DummyBusinessObjectWithInvoicingPlugIn : DummyBusinessObject, IJobInvoicingPlugIn
		{
			public DummyBusinessObjectWithInvoicingPlugIn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IJobInvoicingPlugIn Members

			public IJobInvoicingSupporter InvoicingSupporter
			{
				get { return invoicingSupporter ?? (invoicingSupporter = new JobInvoicingSupporter(this)); }
			}

			JobInvoicingSupporter invoicingSupporter;

			#endregion

			#region IJobHeaderParent Members

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { throw new NotImplementedException(); }
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			ZGuid IJobHeaderParentCore.PK
			{
				get { throw new NotImplementedException(); }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}

			string IJobHeaderParentCore.TableName
			{
				get { throw new NotImplementedException(); }
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IJobNumber Members

			string IJobNumber.JobNumber
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}

		[SupportExRateSource(ExRateSourceType.Voyage)]
		class DummyExRatesSource : DummyBusinessObjectWithInvoicingPlugIn, IJobInvoicingExRateSourceProvider
		{
			public DummyExRatesSource(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IJobInvoicingExRateSourceProvider Members

			IExchangeRateSource IJobInvoicingExRateSourceProvider.GetExRateSource(ExRateSourceType sourceType)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
