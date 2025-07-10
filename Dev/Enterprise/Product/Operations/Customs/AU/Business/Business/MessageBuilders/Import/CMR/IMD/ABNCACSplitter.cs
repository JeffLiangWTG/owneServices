using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ABNCACSplitter
	{
		public ABNCACSplitter(ZString combinedABNCACString)
		{
			this.combinedABNCACString = combinedABNCACString;
		}

		#region Constants

		public const char Delimiter = '/';

		#endregion

		#region ABN

		public ZString ABN
		{
			get
			{
				PerformSplit();
				return fABN;
			}
		}
		ZString fABN;

		#endregion

		#region CAC

		public ZString CAC
		{
			get
			{
				PerformSplit();
				return fCAC;
			}
		}
		ZString fCAC;

		#endregion

		#region IsValid

		public ZBool IsValid
		{
			get
			{
				PerformSplit();
				return fIsValid;
			}
		}
		bool fIsValid;

		#endregion

		#region Implementation

		void PerformSplit()
		{
			if (!isCached)
			{
				ZString spacelessString = combinedABNCACString.Replace(" ", "");
				ZString[] splitString = spacelessString.Split(Delimiter);
				if (splitString.Length == 2)
				{
					CheckAndSetIfValid(splitString[0], splitString[1]);
				}
				else if (splitString.Length == 1)
				{
					if (spacelessString.Length <= 14)
					{
						CheckAndSetIfValid(spacelessString.SubstringSafe(0, 11), spacelessString.SubstringSafe(11, 3));
					}
				}

				isCached = true;
			}
		}

		void CheckAndSetIfValid(ZString potentialABN, ZString potentialCAC)
		{
			if (potentialABN.Length == 11)
			{
				if (potentialCAC.Length == 3)
				{
					SetValues(potentialABN, potentialCAC);
				}
				else if (potentialCAC == ZString.Empty)
				{
					SetValues(potentialABN, potentialCAC);
				}
			}
		}

		void SetValues(ZString aBNToSet, ZString cACToSet)
		{
			fABN = aBNToSet;
			fCAC = cACToSet;
			fIsValid = true;
		}

		bool isCached;
		readonly ZString combinedABNCACString;

		#endregion
	}
}
