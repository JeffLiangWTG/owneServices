using System.Collections.Generic;
using CargoWise.Common.Collections;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocLineSorterTest : TestCase
	{
		public void TestSorter()
		{
			List<DummyDocLine> dummyList = new List<DummyDocLine>();
			int[] ints = { 999, 1, 0 };
			string[] strings = { "", "a", "b" };

			for (int alp = 0; alp < 3; alp++)
			{
				for (int user = 0; user < 3; user++)
				{
					for (int org = 0; org < 3; org++)
					{
						for (int charge = 0; charge < 3; charge++)
						{
							dummyList.Add(new DummyDocLine(ints[org], ints[charge], ints[user], strings[alp]));
						}
					}
				}
			}

			dummyList.StableSort(DocLineSorter.CompareAlphabetically);
			AssertMultilineASCIIEquals("Alphabetic", alpExample, GetIDStringList(dummyList));

			dummyList.StableSort(DocLineSorter.CompareBySequence);
			AssertMultilineASCIIEquals("Sequence", seqExample, GetIDStringList(dummyList));

			dummyList.StableSort(DocLineSorter.CompareByUserEntered);
			AssertMultilineASCIIEquals("User-entered", usrExample, GetIDStringList(dummyList));
		}

		string GetIDStringList(List<DummyDocLine> charges)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (DummyDocLine charge in charges)
			{
				result.Append(charge.ID);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		class DummyDocLine : ISortableDocLine
		{
			public DummyDocLine(int org, int charge, int user, string alp)
			{
				OrgLevelSortOrder = org;
				ChargePrintSeqSortOrder = charge;
				UserEnteredSortOrder = user;
				AlphabeticalSortOrder = alp;
			}

			public string ID
			{
				get
				{
					return string.Format("usr{0}org{1}chr{2}alp'{3}'",
										 UserEnteredSortOrder.ToString().PadRight(5),
										 OrgLevelSortOrder.ToString().PadRight(5),
										 ChargePrintSeqSortOrder.ToString().PadRight(5),
										 AlphabeticalSortOrder);
				}
			}

			public int OrgLevelSortOrder { get; private set; }

			public int ChargePrintSeqSortOrder { get; private set; }

			public int UserEnteredSortOrder { get; private set; }

			public string AlphabeticalSortOrder { get; private set; }
		}

		const string alpExample =
@"usr1    org999  chr999  alp''
usr1    org999  chr1    alp''
usr1    org999  chr0    alp''
usr1    org1    chr999  alp''
usr1    org1    chr1    alp''
usr1    org1    chr0    alp''
usr1    org0    chr999  alp''
usr1    org0    chr1    alp''
usr1    org0    chr0    alp''
usr999  org999  chr999  alp''
usr999  org999  chr1    alp''
usr999  org999  chr0    alp''
usr999  org1    chr999  alp''
usr999  org1    chr1    alp''
usr999  org1    chr0    alp''
usr999  org0    chr999  alp''
usr999  org0    chr1    alp''
usr999  org0    chr0    alp''
usr0    org999  chr999  alp''
usr0    org999  chr1    alp''
usr0    org999  chr0    alp''
usr0    org1    chr999  alp''
usr0    org1    chr1    alp''
usr0    org1    chr0    alp''
usr0    org0    chr999  alp''
usr0    org0    chr1    alp''
usr0    org0    chr0    alp''
usr1    org999  chr999  alp'a'
usr1    org999  chr1    alp'a'
usr1    org999  chr0    alp'a'
usr1    org1    chr999  alp'a'
usr1    org1    chr1    alp'a'
usr1    org1    chr0    alp'a'
usr1    org0    chr999  alp'a'
usr1    org0    chr1    alp'a'
usr1    org0    chr0    alp'a'
usr999  org999  chr999  alp'a'
usr999  org999  chr1    alp'a'
usr999  org999  chr0    alp'a'
usr999  org1    chr999  alp'a'
usr999  org1    chr1    alp'a'
usr999  org1    chr0    alp'a'
usr999  org0    chr999  alp'a'
usr999  org0    chr1    alp'a'
usr999  org0    chr0    alp'a'
usr0    org999  chr999  alp'a'
usr0    org999  chr1    alp'a'
usr0    org999  chr0    alp'a'
usr0    org1    chr999  alp'a'
usr0    org1    chr1    alp'a'
usr0    org1    chr0    alp'a'
usr0    org0    chr999  alp'a'
usr0    org0    chr1    alp'a'
usr0    org0    chr0    alp'a'
usr1    org999  chr999  alp'b'
usr1    org999  chr1    alp'b'
usr1    org999  chr0    alp'b'
usr1    org1    chr999  alp'b'
usr1    org1    chr1    alp'b'
usr1    org1    chr0    alp'b'
usr1    org0    chr999  alp'b'
usr1    org0    chr1    alp'b'
usr1    org0    chr0    alp'b'
usr999  org999  chr999  alp'b'
usr999  org999  chr1    alp'b'
usr999  org999  chr0    alp'b'
usr999  org1    chr999  alp'b'
usr999  org1    chr1    alp'b'
usr999  org1    chr0    alp'b'
usr999  org0    chr999  alp'b'
usr999  org0    chr1    alp'b'
usr999  org0    chr0    alp'b'
usr0    org999  chr999  alp'b'
usr0    org999  chr1    alp'b'
usr0    org999  chr0    alp'b'
usr0    org1    chr999  alp'b'
usr0    org1    chr1    alp'b'
usr0    org1    chr0    alp'b'
usr0    org0    chr999  alp'b'
usr0    org0    chr1    alp'b'
usr0    org0    chr0    alp'b'";

		const string seqExample =
@"usr1    org1    chr1    alp''
usr1    org1    chr1    alp'a'
usr1    org1    chr1    alp'b'
usr999  org1    chr1    alp''
usr999  org1    chr1    alp'a'
usr999  org1    chr1    alp'b'
usr0    org1    chr1    alp''
usr0    org1    chr1    alp'a'
usr0    org1    chr1    alp'b'
usr1    org1    chr999  alp''
usr1    org1    chr999  alp'a'
usr1    org1    chr999  alp'b'
usr999  org1    chr999  alp''
usr999  org1    chr999  alp'a'
usr999  org1    chr999  alp'b'
usr0    org1    chr999  alp''
usr0    org1    chr999  alp'a'
usr0    org1    chr999  alp'b'
usr1    org1    chr0    alp''
usr1    org1    chr0    alp'a'
usr1    org1    chr0    alp'b'
usr999  org1    chr0    alp''
usr999  org1    chr0    alp'a'
usr999  org1    chr0    alp'b'
usr0    org1    chr0    alp''
usr0    org1    chr0    alp'a'
usr0    org1    chr0    alp'b'
usr1    org999  chr1    alp''
usr1    org999  chr1    alp'a'
usr1    org999  chr1    alp'b'
usr999  org999  chr1    alp''
usr999  org999  chr1    alp'a'
usr999  org999  chr1    alp'b'
usr0    org999  chr1    alp''
usr0    org999  chr1    alp'a'
usr0    org999  chr1    alp'b'
usr1    org999  chr999  alp''
usr1    org999  chr999  alp'a'
usr1    org999  chr999  alp'b'
usr999  org999  chr999  alp''
usr999  org999  chr999  alp'a'
usr999  org999  chr999  alp'b'
usr0    org999  chr999  alp''
usr0    org999  chr999  alp'a'
usr0    org999  chr999  alp'b'
usr1    org999  chr0    alp''
usr1    org999  chr0    alp'a'
usr1    org999  chr0    alp'b'
usr999  org999  chr0    alp''
usr999  org999  chr0    alp'a'
usr999  org999  chr0    alp'b'
usr0    org999  chr0    alp''
usr0    org999  chr0    alp'a'
usr0    org999  chr0    alp'b'
usr1    org0    chr1    alp''
usr1    org0    chr1    alp'a'
usr1    org0    chr1    alp'b'
usr999  org0    chr1    alp''
usr999  org0    chr1    alp'a'
usr999  org0    chr1    alp'b'
usr0    org0    chr1    alp''
usr0    org0    chr1    alp'a'
usr0    org0    chr1    alp'b'
usr1    org0    chr999  alp''
usr1    org0    chr999  alp'a'
usr1    org0    chr999  alp'b'
usr999  org0    chr999  alp''
usr999  org0    chr999  alp'a'
usr999  org0    chr999  alp'b'
usr0    org0    chr999  alp''
usr0    org0    chr999  alp'a'
usr0    org0    chr999  alp'b'
usr1    org0    chr0    alp''
usr1    org0    chr0    alp'a'
usr1    org0    chr0    alp'b'
usr999  org0    chr0    alp''
usr999  org0    chr0    alp'a'
usr999  org0    chr0    alp'b'
usr0    org0    chr0    alp''
usr0    org0    chr0    alp'a'
usr0    org0    chr0    alp'b'";

		const string usrExample =
@"usr1    org1    chr1    alp''
usr1    org1    chr999  alp''
usr1    org1    chr0    alp''
usr1    org999  chr1    alp''
usr1    org999  chr999  alp''
usr1    org999  chr0    alp''
usr1    org0    chr1    alp''
usr1    org0    chr999  alp''
usr1    org0    chr0    alp''
usr1    org1    chr1    alp'a'
usr1    org1    chr999  alp'a'
usr1    org1    chr0    alp'a'
usr1    org999  chr1    alp'a'
usr1    org999  chr999  alp'a'
usr1    org999  chr0    alp'a'
usr1    org0    chr1    alp'a'
usr1    org0    chr999  alp'a'
usr1    org0    chr0    alp'a'
usr1    org1    chr1    alp'b'
usr1    org1    chr999  alp'b'
usr1    org1    chr0    alp'b'
usr1    org999  chr1    alp'b'
usr1    org999  chr999  alp'b'
usr1    org999  chr0    alp'b'
usr1    org0    chr1    alp'b'
usr1    org0    chr999  alp'b'
usr1    org0    chr0    alp'b'
usr999  org1    chr1    alp''
usr999  org1    chr999  alp''
usr999  org1    chr0    alp''
usr999  org999  chr1    alp''
usr999  org999  chr999  alp''
usr999  org999  chr0    alp''
usr999  org0    chr1    alp''
usr999  org0    chr999  alp''
usr999  org0    chr0    alp''
usr999  org1    chr1    alp'a'
usr999  org1    chr999  alp'a'
usr999  org1    chr0    alp'a'
usr999  org999  chr1    alp'a'
usr999  org999  chr999  alp'a'
usr999  org999  chr0    alp'a'
usr999  org0    chr1    alp'a'
usr999  org0    chr999  alp'a'
usr999  org0    chr0    alp'a'
usr999  org1    chr1    alp'b'
usr999  org1    chr999  alp'b'
usr999  org1    chr0    alp'b'
usr999  org999  chr1    alp'b'
usr999  org999  chr999  alp'b'
usr999  org999  chr0    alp'b'
usr999  org0    chr1    alp'b'
usr999  org0    chr999  alp'b'
usr999  org0    chr0    alp'b'
usr0    org1    chr1    alp''
usr0    org1    chr999  alp''
usr0    org1    chr0    alp''
usr0    org999  chr1    alp''
usr0    org999  chr999  alp''
usr0    org999  chr0    alp''
usr0    org0    chr1    alp''
usr0    org0    chr999  alp''
usr0    org0    chr0    alp''
usr0    org1    chr1    alp'a'
usr0    org1    chr999  alp'a'
usr0    org1    chr0    alp'a'
usr0    org999  chr1    alp'a'
usr0    org999  chr999  alp'a'
usr0    org999  chr0    alp'a'
usr0    org0    chr1    alp'a'
usr0    org0    chr999  alp'a'
usr0    org0    chr0    alp'a'
usr0    org1    chr1    alp'b'
usr0    org1    chr999  alp'b'
usr0    org1    chr0    alp'b'
usr0    org999  chr1    alp'b'
usr0    org999  chr999  alp'b'
usr0    org999  chr0    alp'b'
usr0    org0    chr1    alp'b'
usr0    org0    chr999  alp'b'
usr0    org0    chr0    alp'b'";
	}
}
