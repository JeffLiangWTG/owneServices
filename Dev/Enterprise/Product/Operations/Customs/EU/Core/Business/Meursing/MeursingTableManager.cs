using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Meursing
{
	public class MeursingTableManager : IMeursingTableManager
	{
		public MeursingTableManager(IMeursingTarget meursingTarget)
		{
			this.meursingTarget = meursingTarget;
			if (meursingTarget != null)
			{
				this.MeursingTable = new MeursingTable(meursingTarget.Factory);
			}
		}
		public void Execute()
		{
			int suffix = CalculateEcSupplementSuffix();
			if (suffix > -1)
			{
				meursingTarget.SetMeursingResult("7" + suffix.ToString("00#"));
			}
			else
			{
				meursingTarget.SetMeursingResult(ZString.Empty);
			}
		}

		int CalculateEcSupplementSuffix()
		{
			// Pure speed, my friends - both to execute and to code.  I would still have to type in a table full of numbers even if I had some fancy-pants data structure/algorithm.

			#region Starch <5
			if (m.StarchGlucose < 5)
			{
				#region Sucrose <5
				if (m.Sucrose < 5)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 0; }
						else if (m.MilkProtein < 6m)
						{ return 20; }
						else if (m.MilkProtein < 18m)
						{ return 40; }
						else if (m.MilkProtein < 30m)
						{ return 60; }
						else if (m.MilkProtein < 60m)
						{ return 80; }
						else
						{ return 800; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 100; }
						else if (m.MilkProtein < 6m)
						{ return 120; }
						else if (m.MilkProtein < 18m)
						{ return 140; }
						else if (m.MilkProtein < 30m)
						{ return 160; }
						else if (m.MilkProtein < 60m)
						{ return 180; }
						else
						{ return 820; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 840; }
						else if (m.MilkProtein < 12m)
						{ return 200; }
						else
						{ return 260; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 860; }
						else if (m.MilkProtein < 15m)
						{ return 300; }
						else
						{ return 360; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 900; }
						else if (m.MilkProtein < 18m)
						{ return 400; }
						else
						{ return 460; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 940; }
						else if (m.MilkProtein < 18m)
						{ return 500; }
						else
						{ return 560; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 960; }
						else
						{ return 600; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 980; }
						else
						{ return 700; }
					}
					else if (m.MilkFat < 55m)
					{ return 720; }
					else if (m.MilkFat < 70m)
					{ return 740; }
					else if (m.MilkFat < 85m)
					{ return 760; }
					else
					{ return 780; }
				}
				#endregion
				#region sucrose 5-30
				else if (m.Sucrose < 30)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 1; }
						else if (m.MilkProtein < 6m)
						{ return 21; }
						else if (m.MilkProtein < 18m)
						{ return 41; }
						else if (m.MilkProtein < 30m)
						{ return 61; }
						else if (m.MilkProtein < 60m)
						{ return 81; }
						else
						{ return 801; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 101; }
						else if (m.MilkProtein < 6m)
						{ return 121; }
						else if (m.MilkProtein < 18m)
						{ return 141; }
						else if (m.MilkProtein < 30m)
						{ return 161; }
						else if (m.MilkProtein < 60m)
						{ return 181; }
						else
						{ return 821; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 841; }
						else if (m.MilkProtein < 12m)
						{ return 201; }
						else
						{ return 261; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 861; }
						else if (m.MilkProtein < 15m)
						{ return 301; }
						else
						{ return 361; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 901; }
						else if (m.MilkProtein < 18m)
						{ return 401; }
						else
						{ return 461; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 941; }
						else if (m.MilkProtein < 18m)
						{ return 501; }
						else
						{ return 561; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 961; }
						else
						{ return 601; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 981; }
						else
						{ return 701; }
					}
					else if (m.MilkFat < 55m)
					{ return 721; }
					else if (m.MilkFat < 70m)
					{ return 741; }
					else if (m.MilkFat < 85m)
					{ return 761; }
					else
					{ return 781; }
				}
				#endregion
				#region sucrose 30-50
				else if (m.Sucrose < 50)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 2; }
						else if (m.MilkProtein < 6m)
						{ return 22; }
						else if (m.MilkProtein < 18m)
						{ return 42; }
						else if (m.MilkProtein < 30m)
						{ return 62; }
						else if (m.MilkProtein < 60m)
						{ return 82; }
						else
						{ return 802; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 102; }
						else if (m.MilkProtein < 6m)
						{ return 122; }
						else if (m.MilkProtein < 18m)
						{ return 142; }
						else if (m.MilkProtein < 30m)
						{ return 162; }
						else if (m.MilkProtein < 60m)
						{ return 182; }
						else
						{ return 822; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 842; }
						else if (m.MilkProtein < 12m)
						{ return 202; }
						else
						{ return 262; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 862; }
						else if (m.MilkProtein < 15m)
						{ return 302; }
						else
						{ return 362; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 902; }
						else if (m.MilkProtein < 18m)
						{ return 402; }
						else
						{ return 462; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 942; }
						else if (m.MilkProtein < 18m)
						{ return 502; }
						else
						{ return 562; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 962; }
						else
						{ return 602; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 982; }
						else
						{ return 702; }
					}
					else if (m.MilkFat < 55m)
					{ return 722; }
					else if (m.MilkFat < 70m)
					{ return 742; }
					else if (m.MilkFat < 85m)
					{ return 762; }
					// No 85+ 
				}
				#endregion
				#region sucrose 50-70
				else if (m.Sucrose < 70)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 3; }
						else if (m.MilkProtein < 6m)
						{ return 23; }
						else if (m.MilkProtein < 18m)
						{ return 43; }
						else if (m.MilkProtein < 30m)
						{ return 63; }
						else if (m.MilkProtein < 60m)
						{ return 83; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 103; }
						else if (m.MilkProtein < 6m)
						{ return 123; }
						else if (m.MilkProtein < 18m)
						{ return 143; }
						else if (m.MilkProtein < 30m)
						{ return 163; }
						else if (m.MilkProtein < 60m)
						{ return 183; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 843; }
						else if (m.MilkProtein < 12m)
						{ return 203; }
						else
						{ return 263; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 863; }
						else if (m.MilkProtein < 15m)
						{ return 303; }
						else
						{ return 363; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 903; }
						else if (m.MilkProtein < 18m)
						{ return 403; }
						else
						{ return 463; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 943; }
						else if (m.MilkProtein < 18m)
						{ return 503; }
						else
						{ return 563; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 963; }
						else
						{ return 603; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 983; }
						else
						{ return 703; }
					}
					else if (m.MilkFat < 55m)
					{ return 723; }
				}
				#endregion
				#region sucrose 70+
				else
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 4; }
						else if (m.MilkProtein < 6m)
						{ return 24; }
						else if (m.MilkProtein < 18m)
						{ return 44; }
						else if (m.MilkProtein < 30m)
						{ return 64; }
						else if (m.MilkProtein < 60m)
						{ return 84; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 104; }
						else if (m.MilkProtein < 6m)
						{ return 124; }
						else if (m.MilkProtein < 18m)
						{ return 144; }
						else if (m.MilkProtein < 30m)
						{ return 164; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 844; }
						else if (m.MilkProtein < 12m)
						{ return 204; }
						else
						{ return 264; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 864; }
						else if (m.MilkProtein < 15m)
						{ return 304; }
						else
						{ return 364; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 904; }
						else if (m.MilkProtein < 18m)
						{ return 404; }
						else
						{ return 464; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 944; }
						else if (m.MilkProtein < 18m)
						{ return 504; }
						else
						{ return 564; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 964; }
						else
						{ return 604; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 984; }
					}
				}
				#endregion
			}
			#endregion
			#region starch 5-25
			else if (m.StarchGlucose < 25)
			{
				#region Sucrose <5
				if (m.Sucrose < 5)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 5; }
						else if (m.MilkProtein < 6m)
						{ return 25; }
						else if (m.MilkProtein < 18m)
						{ return 45; }
						else if (m.MilkProtein < 30m)
						{ return 65; }
						else if (m.MilkProtein < 60m)
						{ return 85; }
						else
						{ return 805; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 105; }
						else if (m.MilkProtein < 6m)
						{ return 125; }
						else if (m.MilkProtein < 18m)
						{ return 145; }
						else if (m.MilkProtein < 30m)
						{ return 165; }
						else if (m.MilkProtein < 60m)
						{ return 185; }
						else
						{ return 825; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 845; }
						else if (m.MilkProtein < 12m)
						{ return 205; }
						else
						{ return 265; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 865; }
						else if (m.MilkProtein < 15m)
						{ return 305; }
						else
						{ return 365; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 905; }
						else if (m.MilkProtein < 18m)
						{ return 405; }
						else
						{ return 465; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 945; }
						else if (m.MilkProtein < 18m)
						{ return 505; }
						else
						{ return 565; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 965; }
						else
						{ return 605; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 985; }
						else
						{ return 705; }
					}
					else if (m.MilkFat < 55m)
					{ return 725; }
					else if (m.MilkFat < 70m)
					{ return 745; }
					else if (m.MilkFat < 85m)
					{ return 765; }
					else
					{ return 785; }
				}
				#endregion
				#region sucrose 5-30
				else if (m.Sucrose < 30)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 6; }
						else if (m.MilkProtein < 6m)
						{ return 26; }
						else if (m.MilkProtein < 18m)
						{ return 46; }
						else if (m.MilkProtein < 30m)
						{ return 66; }
						else if (m.MilkProtein < 60m)
						{ return 86; }
						else
						{ return 806; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 106; }
						else if (m.MilkProtein < 6m)
						{ return 126; }
						else if (m.MilkProtein < 18m)
						{ return 146; }
						else if (m.MilkProtein < 30m)
						{ return 166; }
						else if (m.MilkProtein < 60m)
						{ return 186; }
						else
						{ return 826; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 846; }
						else if (m.MilkProtein < 12m)
						{ return 206; }
						else
						{ return 266; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 866; }
						else if (m.MilkProtein < 15m)
						{ return 306; }
						else
						{ return 366; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 906; }
						else if (m.MilkProtein < 18m)
						{ return 406; }
						else
						{ return 466; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 946; }
						else if (m.MilkProtein < 18m)
						{ return 506; }
						else
						{ return 566; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 966; }
						else
						{ return 606; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 986; }
						else
						{ return 706; }
					}
					else if (m.MilkFat < 55m)
					{ return 726; }
					else if (m.MilkFat < 70m)
					{ return 746; }
					else if (m.MilkFat < 85m)
					{ return 766; }
					else
					{ return 786; }
				}
				#endregion
				#region sucrose 30-50
				else if (m.Sucrose < 50)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 7; }
						else if (m.MilkProtein < 6m)
						{ return 27; }
						else if (m.MilkProtein < 18m)
						{ return 47; }
						else if (m.MilkProtein < 30m)
						{ return 67; }
						else if (m.MilkProtein < 60m)
						{ return 87; }
						else
						{ return 807; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 107; }
						else if (m.MilkProtein < 6m)
						{ return 127; }
						else if (m.MilkProtein < 18m)
						{ return 147; }
						else if (m.MilkProtein < 30m)
						{ return 167; }
						else if (m.MilkProtein < 60m)
						{ return 187; }
						else
						{ return 827; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 847; }
						else if (m.MilkProtein < 12m)
						{ return 207; }
						else
						{ return 267; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 867; }
						else if (m.MilkProtein < 15m)
						{ return 307; }
						else
						{ return 367; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 907; }
						else if (m.MilkProtein < 18m)
						{ return 407; }
						else
						{ return 467; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 947; }
						else if (m.MilkProtein < 18m)
						{ return 507; }
						else
						{ return 567; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 967; }
						else
						{ return 607; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 987; }
						else
						{ return 707; }
					}
					else if (m.MilkFat < 55m)
					{ return 727; }
					else if (m.MilkFat < 70m)
					{ return 747; }
					// No 70+ 
				}
				#endregion
				#region sucrose 50-70
				else if (m.Sucrose < 70)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 8; }
						else if (m.MilkProtein < 6m)
						{ return 28; }
						else if (m.MilkProtein < 18m)
						{ return 48; }
						else if (m.MilkProtein < 30m)
						{ return 68; }
						else if (m.MilkProtein < 60m)
						{ return 88; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 108; }
						else if (m.MilkProtein < 6m)
						{ return 128; }
						else if (m.MilkProtein < 18m)
						{ return 148; }
						else if (m.MilkProtein < 30m)
						{ return 168; }
						else if (m.MilkProtein < 60m)
						{ return 188; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 848; }
						else if (m.MilkProtein < 12m)
						{ return 208; }
						else
						{ return 268; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 868; }
						else if (m.MilkProtein < 15m)
						{ return 308; }
						else
						{ return 368; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 908; }
						else if (m.MilkProtein < 18m)
						{ return 408; }
						else
						{ return 468; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 948; }
						else if (m.MilkProtein < 18m)
						{ return 508; }
						else
						{ return 568; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 968; }
						else
						{ return 608; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 988; }
						else
						{ return 708; }
					}
					else if (m.MilkFat < 55m)
					{ return 728; }
				}
				#endregion
				#region sucrose 70+
				else
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 9; }
						else if (m.MilkProtein < 6m)
						{ return 29; }
						else if (m.MilkProtein < 18m)
						{ return 49; }
						else if (m.MilkProtein < 30m)
						{ return 69; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 109; }
						else if (m.MilkProtein < 6m)
						{ return 129; }
						else if (m.MilkProtein < 18m)
						{ return 149; }
						else if (m.MilkProtein < 30m)
						{ return 169; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 849; }
						else if (m.MilkProtein < 12m)
						{ return 209; }
						else
						{ return 269; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 869; }
						else if (m.MilkProtein < 15m)
						{ return 309; }
						else
						{ return 369; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 909; }
						else if (m.MilkProtein < 18m)
						{ return 409; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 949; }
						else if (m.MilkProtein < 18m)
						{ return 509; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 969; }
						else
						{ return 609; }
					}
				}
				#endregion
			}
			#endregion
			#region Starch 25-50
			else if (m.StarchGlucose < 50)
			{
				#region Sucrose <5
				if (m.Sucrose < 5)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 10; }
						else if (m.MilkProtein < 6m)
						{ return 30; }
						else if (m.MilkProtein < 18m)
						{ return 50; }
						else if (m.MilkProtein < 30m)
						{ return 70; }
						else if (m.MilkProtein < 60m)
						{ return 90; }
						else
						{ return 810; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 110; }
						else if (m.MilkProtein < 6m)
						{ return 130; }
						else if (m.MilkProtein < 18m)
						{ return 150; }
						else if (m.MilkProtein < 30m)
						{ return 170; }
						else if (m.MilkProtein < 60m)
						{ return 190; }
						else
						{ return 830; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 850; }
						else if (m.MilkProtein < 12m)
						{ return 210; }
						else
						{ return 270; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 870; }
						else if (m.MilkProtein < 15m)
						{ return 310; }
						else
						{ return 370; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 910; }
						else if (m.MilkProtein < 18m)
						{ return 410; }
						else
						{ return 470; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 950; }
						else if (m.MilkProtein < 18m)
						{ return 510; }
						else
						{ return 570; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 970; }
						else
						{ return 610; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 990; }
						else
						{ return 710; }
					}
					else if (m.MilkFat < 55m)
					{ return 730; }
					else if (m.MilkFat < 70m)
					{ return 750; }
					else if (m.MilkFat < 85m)
					{ return 770; }
				}
				#endregion
				#region sucrose 5-30
				else if (m.Sucrose < 30)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 11; }
						else if (m.MilkProtein < 6m)
						{ return 31; }
						else if (m.MilkProtein < 18m)
						{ return 51; }
						else if (m.MilkProtein < 30m)
						{ return 71; }
						else if (m.MilkProtein < 60m)
						{ return 91; }
						else
						{ return 811; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 111; }
						else if (m.MilkProtein < 6m)
						{ return 131; }
						else if (m.MilkProtein < 18m)
						{ return 151; }
						else if (m.MilkProtein < 30m)
						{ return 171; }
						else if (m.MilkProtein < 60m)
						{ return 191; }
						else
						{ return 831; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 851; }
						else if (m.MilkProtein < 12m)
						{ return 211; }
						else
						{ return 271; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 871; }
						else if (m.MilkProtein < 15m)
						{ return 311; }
						else
						{ return 371; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 911; }
						else if (m.MilkProtein < 18m)
						{ return 411; }
						else
						{ return 471; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 951; }
						else if (m.MilkProtein < 18m)
						{ return 511; }
						else
						{ return 571; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 971; }
						else
						{ return 611; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 991; }
						else
						{ return 711; }
					}
					else if (m.MilkFat < 55m)
					{ return 731; }
					else if (m.MilkFat < 70m)
					{ return 751; }
					else if (m.MilkFat < 85m)
					{ return 771; }
				}
				#endregion
				#region sucrose 30-50
				else if (m.Sucrose < 50)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 12; }
						else if (m.MilkProtein < 6m)
						{ return 32; }
						else if (m.MilkProtein < 18m)
						{ return 52; }
						else if (m.MilkProtein < 30m)
						{ return 72; }
						else if (m.MilkProtein < 60m)
						{ return 92; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 112; }
						else if (m.MilkProtein < 6m)
						{ return 132; }
						else if (m.MilkProtein < 18m)
						{ return 152; }
						else if (m.MilkProtein < 30m)
						{ return 172; }
						else if (m.MilkProtein < 60m)
						{ return 192; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 852; }
						else if (m.MilkProtein < 12m)
						{ return 212; }
						else
						{ return 272; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 872; }
						else if (m.MilkProtein < 15m)
						{ return 312; }
						else
						{ return 372; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 912; }
						else if (m.MilkProtein < 18m)
						{ return 412; }
						else
						{ return 472; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 952; }
						else if (m.MilkProtein < 18m)
						{ return 512; }
						else
						{ return 572; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 972; }
						else
						{ return 612; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 992; }
						else
						{ return 712; }
					}
					else if (m.MilkFat < 55m)
					{ return 732; }
				}
				#endregion
				#region sucrose 50+
				else
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 13; }
						else if (m.MilkProtein < 6m)
						{ return 33; }
						else if (m.MilkProtein < 18m)
						{ return 53; }
						else if (m.MilkProtein < 30m)
						{ return 73; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 113; }
						else if (m.MilkProtein < 6m)
						{ return 133; }
						else if (m.MilkProtein < 18m)
						{ return 153; }
						else if (m.MilkProtein < 30m)
						{ return 173; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 853; }
						else if (m.MilkProtein < 12m)
						{ return 213; }
						else
						{ return 273; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 873; }
						else if (m.MilkProtein < 15m)
						{ return 313; }
						else
						{ return 373; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 913; }
						else if (m.MilkProtein < 18m)
						{ return 413; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 953; }
						else if (m.MilkProtein < 18m)
						{ return 513; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 973; }
						else
						{ return 613; }
					}
				}
				#endregion
			}
			#endregion
			#region starch 50-75
			else if (m.StarchGlucose < 75)
			{
				#region Sucrose <5
				if (m.Sucrose < 5)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 15; }
						else if (m.MilkProtein < 6m)
						{ return 35; }
						else if (m.MilkProtein < 18m)
						{ return 55; }
						else if (m.MilkProtein < 30m)
						{ return 75; }
						else if (m.MilkProtein < 60m)
						{ return 95; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 115; }
						else if (m.MilkProtein < 6m)
						{ return 135; }
						else if (m.MilkProtein < 18m)
						{ return 155; }
						else if (m.MilkProtein < 30m)
						{ return 175; }
						else if (m.MilkProtein < 60m)
						{ return 195; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 855; }
						else if (m.MilkProtein < 12m)
						{ return 215; }
						else
						{ return 275; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 875; }
						else if (m.MilkProtein < 15m)
						{ return 315; }
						else
						{ return 375; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 915; }
						else if (m.MilkProtein < 18m)
						{ return 415; }
						else
						{ return 475; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 955; }
						else if (m.MilkProtein < 18m)
						{ return 515; }
						else
						{ return 575; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 975; }
						else
						{ return 615; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 995; }
						else
						{ return 715; }
					}
					else if (m.MilkFat < 55m)
					{ return 735; }
				}
				#endregion
				#region sucrose 5-30
				else if (m.Sucrose < 30)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 16; }
						else if (m.MilkProtein < 6m)
						{ return 36; }
						else if (m.MilkProtein < 18m)
						{ return 56; }
						else if (m.MilkProtein < 30m)
						{ return 76; }
						else if (m.MilkProtein < 60m)
						{ return 96; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 116; }
						else if (m.MilkProtein < 6m)
						{ return 136; }
						else if (m.MilkProtein < 18m)
						{ return 156; }
						else if (m.MilkProtein < 30m)
						{ return 176; }
						else if (m.MilkProtein < 60m)
						{ return 196; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 856; }
						else if (m.MilkProtein < 12m)
						{ return 216; }
						else
						{ return 276; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 876; }
						else if (m.MilkProtein < 15m)
						{ return 316; }
						else
						{ return 376; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 916; }
						else if (m.MilkProtein < 18m)
						{ return 416; }
						else
						{ return 476; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 956; }
						else if (m.MilkProtein < 18m)
						{ return 516; }
						else
						{ return 576; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 976; }
						else
						{ return 616; }
					}
					else if (m.MilkFat < 40m)
					{
						if (m.MilkProtein < 6m)
						{ return 996; }
						else
						{ return 716; }
					}
					else if (m.MilkFat < 55m)
					{ return 736; }
				}
				#endregion
				#region sucrose 30+
				else
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 17; }
						else if (m.MilkProtein < 6m)
						{ return 37; }
						else if (m.MilkProtein < 18m)
						{ return 57; }
						else if (m.MilkProtein < 30m)
						{ return 77; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 117; }
						else if (m.MilkProtein < 6m)
						{ return 137; }
						else if (m.MilkProtein < 18m)
						{ return 157; }
						else if (m.MilkProtein < 30m)
						{ return 177; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 857; }
						else if (m.MilkProtein < 12m)
						{ return 217; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 877; }
						else if (m.MilkProtein < 15m)
						{ return 317; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 917; }
						else if (m.MilkProtein < 18m)
						{ return 417; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 957; }
						else if (m.MilkProtein < 18m)
						{ return 517; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 977; }
					}
				}
				#endregion
			}
			#endregion
			#region Starch 75+
			else
			{
				#region Sucrose <5
				if (m.Sucrose < 5)
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 758; }
						else if (m.MilkProtein < 6m)
						{ return 768; }
						else if (m.MilkProtein < 18m)
						{ return 778; }
						else if (m.MilkProtein < 30m)
						{ return 788; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 798; }
						else if (m.MilkProtein < 6m)
						{ return 808; }
						else if (m.MilkProtein < 18m)
						{ return 818; }
						else if (m.MilkProtein < 30m)
						{ return 828; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 858; }
						else if (m.MilkProtein < 12m)
						{ return 220; }
						else
						{ return 838; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 878; }
						else if (m.MilkProtein < 15m)
						{ return 320; }
						else
						{ return 378; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 918; }
						else if (m.MilkProtein < 18m)
						{ return 420; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 958; }
						else if (m.MilkProtein < 18m)
						{ return 520; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 978; }
						else
						{ return 620; }
					}
				}
				#endregion
				#region sucrose 5+
				else
				{
					if (m.MilkFat < 1.5m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 759; }
						else if (m.MilkProtein < 6m)
						{ return 769; }
						else if (m.MilkProtein < 18m)
						{ return 779; }
						else if (m.MilkProtein < 30m)
						{ return 789; }
					}
					else if (m.MilkFat < 3m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 799; }
						else if (m.MilkProtein < 6m)
						{ return 809; }
						else if (m.MilkProtein < 18m)
						{ return 819; }
						else if (m.MilkProtein < 30m)
						{ return 829; }
					}
					else if (m.MilkFat < 6m)
					{
						if (m.MilkProtein < 2.5m)
						{ return 859; }
						else if (m.MilkProtein < 12m)
						{ return 221; }
					}
					else if (m.MilkFat < 9m)
					{
						if (m.MilkProtein < 4m)
						{ return 879; }
						else if (m.MilkProtein < 15m)
						{ return 321; }
					}
					else if (m.MilkFat < 12m)
					{
						if (m.MilkProtein < 6m)
						{ return 919; }
						else if (m.MilkProtein < 18m)
						{ return 421; }
					}
					else if (m.MilkFat < 18m)
					{
						if (m.MilkProtein < 6m)
						{ return 959; }
						else if (m.MilkProtein < 18m)
						{ return 521; }
					}
					else if (m.MilkFat < 26m)
					{
						if (m.MilkProtein < 6m)
						{ return 979; }
					}
				}
				#endregion
			}
			#endregion

			return -1;
		}

		MeursingTable m { get { return MeursingTable; } }

		public IMeursingTarget meursingTarget { get; }

		public MeursingTable MeursingTable { get; private set; }
	}
}

