using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRLodgementQuestionComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			TestCaseHelper.ClearTable(AutoCMRLodgementQuestion.Schema.TableName);
			var currentDate = ZDateTime.Today;
			var lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 1;
			lodgementQuestion.CQ_LodgementQuestionStartDate = currentDate.AddDays(-100);
			lodgementQuestion.CQ_LodgementQuestionEndDate = currentDate.AddDays(2);

			var lodgementQuestion1 = CMRLodgementQuestion.New(Factory);
			lodgementQuestion1.CQ_LodgementQuestionIdentifier = 1;
			lodgementQuestion1.CQ_LodgementQuestionStartDate = currentDate.AddDays(-10);

			Factory.Save();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var questions = CMRLodgementQuestion.Load(new OrganisationCPQA(org), new ZInt[] { 1 });
			AssertEquals("Precondition", 2, questions.Length);
			Array.Sort(questions, new CMRLodgementQuestionComparer());
			AssertEquals(lodgementQuestion.PK, questions[0].PK);
			AssertEquals(lodgementQuestion1.PK, questions[1].PK);
		}
	}
}
