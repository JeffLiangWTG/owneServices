using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	class fn_MeanVarianceSkewIntegrationTest : TransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestMeanVariance()
		{
			string mvsquery = @"select * from 
			(
			select name, min(dateFromUtc) DateFrom, max(DateToUtc) DateTo, sum(actionCount) ActionCount, sum(sumofseconds) SumOfSeconds, sum(SumOfSquaresSeconds) SumOfSquaresSeconds, 
			sum(SumOfCubesSeconds) SumOfCubesSeconds from dbo.vw_StatisticsMeasurementsIncludingChildren
			where dateFromUtc >= '2012-08-01'
			group by Name, cast(dateFromUtc as date)) IQ
			cross apply fn_MeanVarianceSkew (ActionCount, SumOfSeconds, SumOfSquaresSeconds, SumOfCubesSeconds) OQ
			order by name, datefrom";

			decimal[] population = { 177M, 332M, 648M, 633M, 894M, 509M, 786M, 993M, 162M, 270M, 802M, 903M, 322M, 664M, 718M, 564M, 512M, 705M, 338M, 96M, 657M, 683M, 49M, 222M, 653M, 673M, 907M };
			decimal[] mean = { 177M, 254.5M, 385.6666667M, 447.5M, 536.8M, 532.1666667M, 568.4285714M, 621.5M, 570.4444444M, 540.4M, 564.1818182M, 592.4166667M, 571.6153846M, 578.2142857M, 587.5333333M, 586.0625M, 581.7058824M, 588.5555556M, 575.3684211M, 551.4M, 556.4285714M, 562.1818182M, 539.8695652M, 526.625M, 531.68M, 537.1153846M, 550.8148148M };
			decimal[] variance = { 0.0M, 12012.5M, 57620.33333M, 53707M, 80152.7M, 64250.96667M, 62746.95238M, 76315.71429M, 90236.27778M, 89236.71111M, 86534.36364M, 88234.08333M, 86506.25641M, 80461.56593M, 76016.98095M, 70983.79583M, 66869.97059M, 63780.96732M, 63541.69006M, 71687.09474M, 68633.75714M, 66093.67965M, 74539.66403M, 75508.85326M, 73001.47667M, 70849.54615M, 73191.77208M };
			decimal[] skewness = { 0.0M, 0.0M, 0.955784629M, -0.369052308M, -0.134537562M, -0.053097952M, -0.41295539M, -0.339107658M, -0.135093M, 0.118556383M, -0.101928879M, -0.269782639M, -0.076227401M, -0.155772445M, -0.262091032M, -0.249634395M, -0.19820158M, -0.28264576M, -0.15209755M, -0.155925953M, -0.21732512M, -0.285590308M, -0.288093748M, -0.184760616M, -0.241968961M, -0.302033679M, -0.333789574M };
			decimal meanepsilon = 0.0000001M;
			decimal varianceepsilon = 0.00001M;
			decimal skewnessepsilon = 0.00000001M;
			var now = new DateTime(2012, 08, 01);
			var factory = new BusinessObjectFactory();

			for (int i = 0; i < population.Length; ++i)
			{
				for (int j = 0; j <= i; ++j)
				{
					StatisticsTestHelper.CreateUsage(factory, now, now, "1.1.1." + i, "EDI", "BNE", "E", "Name " + i, "SubName " + i, 1, population[j]);
				}
				factory.Save();
				StatisticsTestHelper.ExecStatisticsServiceSummarise(i + 1);
			}

			using (var cmd = Db.Connection.Command(mvsquery))
			{
				IDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var record = (IDataRecord)reader;
					var row = new object[record.FieldCount];
					record.GetValues(row);
					int index = (int)(long)row[3] - 1;
					Assert("Mean", Math.Abs(mean[index] - (decimal)(double)row[7]) < meanepsilon);
					Assert("Variance", Math.Abs(variance[index] - (decimal)(double)row[8]) < varianceepsilon);
					Assert("Skewness", Math.Abs(skewness[index] - (decimal)(double)row[9]) < skewnessepsilon);
				}
				reader.Close();
			}
		}
	}
}
