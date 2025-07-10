using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BlobFetchHintTest : TestCaseWithFactory
	{
		const int BLOB_SIZE = 10 * 1024;

		public void TestDBHitsForLoadWithBlob_LargeBlobLoadingWithFetchHints()
		{
			var random = new Random();
			var bigByteArray = Enumerable.Range(0, 100000).Select(i => (byte)(random.Next() % 254)).ToArray();
			var blob1 = new ZBlob(bigByteArray);
			var bizOSave1 = Factory.New<DummyBusinessObject>();
			bizOSave1.Z0_VarBinaryMax = blob1;
			var bizOSave2 = Factory.New<DummyBusinessObject>();
			bizOSave2.Z0_VarBinaryMax = blob1;
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();

			AssertEquals("No hit yet.", 0, factoryForLoad.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			var query = new ZQuery(DummyBizoSchema.PK, new[] { bizOSave1.PK, bizOSave2.PK });
			var dummies = factoryForLoad.Load<DummyBusinessObject>(query);
			AssertEquals("One hit from the Factory load.", 1, factoryForLoad.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			var queryWithBlob = query.DeepClone();
			queryWithBlob.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			factoryForLoad.AddFetchHint(DummyBizoSchema.Instance, queryWithBlob);

			AssertEquals("Only one hit becausing adding a fetch hint should not cause load", 1, factoryForLoad.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			foreach (var dummy in dummies)
			{
				AssertNotNull(dummy.Z0_VarBinaryMax);
			}

			AssertEquals("Two, because we should not get multiple hits on the blob columns DUE TO the fetch hint.", 2, factoryForLoad.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithoutBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(2, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_AddHintThenWithoutBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithoutBlobThenAddHintThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(2, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithoutBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN2 = loadingFactory.Load<DummyBusinessObject>(queryN2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_AddHintThenWithoutBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN2 = loadingFactory.Load<DummyBusinessObject>(queryN2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithoutBlobThenAddHintThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN2 = loadingFactory.Load<DummyBusinessObject>(queryN2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_AddHintThenWithBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithBlobThenAddHintThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pk);
			DummyBusinessObject bizOResultN = loadingFactory.Load<DummyBusinessObject>(queryN)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pk);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY2 = loadingFactory.Load<DummyBusinessObject>(queryY2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY2.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_AddHintThenWithBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pk);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY2 = loadingFactory.Load<DummyBusinessObject>(queryY2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY2.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithBlob_WithBlobThenAddHintThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			Factory.Save();
			ZGuid pk = bizO.PK;

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pk);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY = loadingFactory.Load<DummyBusinessObject>(queryY)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			FetchHint fetchHint = new FetchHint(DummyBizoSchema.PK, pk, DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(fetchHint);

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pk);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject bizOResultY2 = loadingFactory.Load<DummyBusinessObject>(queryY2)[0];
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultY2.Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithoutBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(2, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(3, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_AddHintThenWithoutBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithoutBlobThenAddHintThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(2, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(2, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithoutBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN2 = loadingFactory.Load<DummyBusinessObject>(queryN2);
			AssertEquals(2, bizOResultsN2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_AddHintThenWithoutBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN2 = loadingFactory.Load<DummyBusinessObject>(queryN2);
			AssertEquals(2, bizOResultsN2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithoutBlobThenAddHintThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			ZQuery queryN2 = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN2 = loadingFactory.Load<DummyBusinessObject>(queryN2);
			AssertEquals(2, bizOResultsN2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_AddHintThenWithBlobThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertEquals(2, bizOResultsY.Length);
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithBlobThenAddHintThenWithoutBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);

			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			ZQuery queryN = new ZQuery(DummyBizoSchema.PK, pks);
			DummyBusinessObject[] bizOResultsN = loadingFactory.Load<DummyBusinessObject>(queryN);
			AssertEquals(2, bizOResultsN.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pks);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY2 = loadingFactory.Load<DummyBusinessObject>(queryY2);
			AssertEquals(2, bizOResultsY2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_AddHintThenWithBlobThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pks);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY2 = loadingFactory.Load<DummyBusinessObject>(queryY2);
			AssertEquals(2, bizOResultsY2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestDBHitsForLoadWithTwoBlobs_WithBlobThenAddHintThenWithBlob()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 1);
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_VarBinaryMax = NewBytes(BLOB_SIZE, 2);
			Factory.Save();
			ZGuid[] pks = new ZGuid[] { bizO.PK, bizO2.PK };

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();

			ZQuery queryY = new ZQuery(DummyBizoSchema.PK, pks);
			queryY.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY = loadingFactory.Load<DummyBusinessObject>(queryY);
			AssertEquals(2, bizOResultsY.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			loadingFactory.AddFetchHint(DummyBizoSchema.Instance, queryY);

			ZQuery queryY2 = new ZQuery(DummyBizoSchema.PK, pks);
			queryY2.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			DummyBusinessObject[] bizOResultsY2 = loadingFactory.Load<DummyBusinessObject>(queryY2);
			AssertEquals(2, bizOResultsY2.Length);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[0].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertNotNull(bizOResultsY2[1].Z0_VarBinaryMax);
			AssertEquals(1, loadingFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		byte[] NewBytes(int length, int fillwith)
		{
			byte[] bytes = new byte[length];
			new Random(length + length).NextBytes(bytes);
			return bytes;
		}
	}
}
