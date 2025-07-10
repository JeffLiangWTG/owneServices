using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.Testing
{
	public abstract class FrontEndMessagingViaCspWebServiceTests<T> : TestCaseWithFactory
	{
		[TestDate(2016, 5, 19)]
		public abstract void TestFrontEndMessaging();
		public abstract void TestFrontEndMessagingShowsNewEntryDetails();
		public abstract void TestFrontEndMessagingShowsSuppressedEntryDetailsIfUnchanged();
		public abstract void TestFrontEndMessagingTwoUploadsAndTwoResponses();
		public abstract void TestMultiEntryDeclarationProcessesBothResponses();
		public abstract void TestFrontEndMessaging_UploadError();
		public abstract void TestFrontEndMessaging_ParserError();
		public abstract void SetupEverything();

		protected override void SetUp()
		{
			base.SetUp();
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			Factory.Save();
		}

		protected abstract void CspSpecificSetup();
		protected abstract string ApplicationCodeForAlreadyQueuedMessage { get; }

		protected Business.GbDeclarationSenderChooser senderChooser;
		protected ZString cusResWithSysCar27 = @"UNA\#.? {UNB#UNOA\1#FCPSYS#FCPCAW\\FCPCAW#130306\1515#130306151545575{UNH#10470501450592#CUSRES\D\04A\UN\109730#<<SYSCAR>>{BGM#EFD\\109##27{RFF#ABO\3GB945390992000-B00001402{ERP#\2\2#RFF\10\5{ERC#6\\109{FTX#AAO###E382 Duplicate UCR detected{DOC#960#B00001402{UNT#8#10470501450592{UNZ#1#130306151545575{";
		protected ZString cusResWithSysCar29 = @"UNA\#.? {UNB#UNOA\1#FCPSYS#FCPCAW\\FCPCAW#130306\1515#130306151545575{UNH#11057030907218#CUSRES\D\04A\UN\109701#<<SYSCAR>>{BGM#IFD\\109##29{DTM#7\201501141112\203{DTM#148\201501141144\203{LOC#14#GBLHRERT\\109{LOC#22#120\\109{LOC#44#071\\109{RFF#ABT\000511J\3{RFF#ABO\5GB945390992000-B00030556\K{RFF#ABS\00{RFF#AHZ\\6{DOC#960#B00030556{MOA#40\1.00{MOA#55\0.00{MOA#1\1.00{MOA#150\0.00{MOA#9\0.00{MOA#74\0.00{MOA#176\0.00{MOA#39\1.00\GBP{CUX###1.00000000{TAX#3#A00#D{MOA#161\0.00{TAX#3#B00{MOA#161\0.00{CST#1{TAX#5{MOA#159\1.00{MOA#40\1.00{MOA#55\0.00{MOA#1\1.00{MOA#150\0.00{MOA#38\1.00{TAX#1#A00#D##F{MOA#161\0.00{TAX#1#B00###Z{MOA#161\0.00{UNT#38#11057030907218{UNZ#1#130306151545575{";
		protected ZString cusResWithSysCar2
		{
			get { return cusResWithSysCar27.Replace("130306151545575", "InterchangeTwo").Replace("10470501450592", "Message2"); }
		}

		protected Customs.Business.SendsMessagesToCustomsShutterUpperer sendGui;
		protected Business.Declaration.JobDeclaration declaration;
		protected Business.Declaration.CusEntryHeader entry;
	}
}
