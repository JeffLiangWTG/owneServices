using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Accounting.Integration.Testing
{
	public class IAutoRatingAndJobInvoicingTest : TestCaseWithFactory
	{
		class MockIAutoRatingAndJobInvoicing : NonPersistentBusinessObject, ICustomsJobInfo
		{
			#region IJobHeaderParent Members

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			#endregion

			#region IJobInvoicingPlugIn Members

			public IJobInvoicingSupporter InvoicingSupporter
			{
				get { return invoicingSupporter ?? (invoicingSupporter = new JobInvoicingSupporter(this)); }
			}

			JobInvoicingSupporter invoicingSupporter;

			#endregion

			public GlbBranch Branch
			{
				get { return GlbBranch.CurrentBranch; }
			}

			public IJobInvoicingPlugIn TopLevelObjectForJobToReference
			{
				get { return this; }
			}

			public AutoPostingNotification AutoPostingNotification
			{
				get { throw new NotImplementedException(); }
			}

			public ZString[] GetValidAPInvoiceNumsToMatchAndValidateAgainst()
			{
				throw new NotImplementedException();
			}

			public ZGuid CreditorPK
			{
				get { throw new NotImplementedException(); }
			}

			#region IJobHeaderParent Members

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { throw new NotImplementedException(); }
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

			#endregion

			#region IJobNumber Members

			string IJobNumber.JobNumber
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IRatingSupporter Members

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get { return new MockIAutoRatingAndJobInvoicingRatingAdaptersProvider(this); }
			}

			#endregion

			#region IImportExport Members

			public Directions JobDirection
			{
				get { return Directions.Unknown; }
			}

			#endregion

			public EntryInfoCollection Entries
			{
				get { throw new NotImplementedException(); }
			}
		}

		class MockIAutoRatingAndJobInvoicingRatingAdaptersProvider : RatingAdaptersProvider<MockIAutoRatingAndJobInvoicing>
		{
			public MockIAutoRatingAndJobInvoicingRatingAdaptersProvider(MockIAutoRatingAndJobInvoicing parent) : base(parent)
			{
			}

			protected override List<IAutoRating> GetAdapters(MockIAutoRatingAndJobInvoicing parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
			{
				return new List<IAutoRating> { new MockIAutoRatingAndJobInvoicingRatingAdapter<MockIAutoRatingAndJobInvoicing>(parent) };
			}
		}

		class MockIAutoRatingAndJobInvoicingRatingAdapter<T> : RatingAdapter<T>, IAutoRatingCustomsInfo
			where T : MockIAutoRatingAndJobInvoicing
		{
			public MockIAutoRatingAndJobInvoicingRatingAdapter(T parent)
				: base(parent)
			{
			}

			#region RatingAdapter

			public override IJobInvoicingSupporter InvoicingSupporter
			{
				get { return Parent.InvoicingSupporter; }
			}

			public override MergeChargeOptions MergeCharges
			{
				get { return MergeChargeOptions.WithinAdapter; }
			}

			public override Directions JobDirection
			{
				get { return Parent.JobDirection; }
			}

			#endregion

			#region IAutoRatingCustomsInfo Members

			ZString IAutoRatingCustomsInfo.MessageType
			{
				get { throw new NotImplementedException(); }
			}

			ZString IAutoRatingCustomsInfo.MessageSubType
			{
				get { throw new NotImplementedException(); }
			}

			EntryInfoCollection IAutoRatingCustomsInfo.Entries
			{
				get { throw new NotImplementedException(); }
			}

			InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
			{
				get { throw new NotImplementedException(); }
			}

			InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
			{
				get { throw new NotImplementedException(); }
			}

			InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
			{
				get { throw new NotImplementedException(); }
			}

			ZInt IAutoRatingCustomsInfo.SubHeaderCount
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}

		public void TestInstantiation()
		{
			var testCreator = new CustomsDisbursementChargePosterCreator();
			var testResult = testCreator.GetNewChargePoster(ChargePosterBehaviours.AutoRateDSB, Array.Empty<ZGuid>());

			AssertNotNull(testResult);
		}
	}
}
