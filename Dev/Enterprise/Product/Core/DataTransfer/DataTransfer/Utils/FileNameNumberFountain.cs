using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	/* As of 10 April 2008, this class is used by client specific solutions (CSP) ONLY, yet resides in base and to designed to
	 * be used by base classes, forcing the CSPs to override the New() contstructor and initialise in their respective 
	 * ClientOverride classes.
	 * 
	 * This means this class can only be instantiated once, and therefore inherited once in the CSP.  And with the use of 
	 * Static methods, allows only ONE FileNameNumberFountain per CSP even though mulitple NumberFountains are supported by 
	 * the NumberFountainFactory.
	 * 
	 * In addition, properties like FountainName, MinValue, MaxValue and (inherited in one CSP solution) Prefix are already 
	 * supported by the base NumberFountains namely FormattedNumberFountain and UnformattedNumberFountaion and therefore should 
	 * not be part of this provider class.  We don't need a FileNameNumberFountain as this feature is already support in the 
	 * FormattedNumberFountain which now has a new suffix property.
	 * 
	 * A new SharedNumberFountainsProvider replaces this class in the ClientSharedComponents solution and overcomes all of 
	 * these issues specifically allowing mulitple NumberFountains of either type.  You can use the NumberFountains provide by
	 * base, or inherit from base and add you own default values.  These fountains are then either added once into the 
	 * SharedNumberFountainsProvider in ClientOverride.InitialiseCore() or as required (allowing use of a run-time key) 
	 * directly into the SharedNumberFountainsProvider.Instance.NumberFountainFactories dictionary<string, INumberFountain>.
	 * 
	 * Only when base also require access to SharedFileNameNumberFountain, should it then be moved to DataTransfer.Business. 
	 *
	 *                              IN THE MEAN TIME, PLEASE DO NOT USE THIS CLASS ANY LONGER.  
	 *                              (See ClientSharedComponents and ZClientWCB for more info) 
	 */

	public class FileNameNumberFountain
	{
		protected FileNameNumberFountain()
		{
		}

		protected delegate FileNameNumberFountain ConstructorDelegate();

		protected static readonly Overridable<ConstructorDelegate> OverridableNewDelegate = new Overridable<ConstructorDelegate>();

		public static FileNameNumberFountain New()
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden == null ? new FileNameNumberFountain() : overridden();
		}

		public static string PeekNewFileName()
		{
			return New().GenerateFilename(Db.Connection, false);
		}

		public static string GetNewFileName()
		{
			ZString fileName;
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				fileName = New().GenerateFilename(Db.Connection, true);

				if (File.Exists(fileName))
				{
					manager.RollbackTransaction();
					fileName = ZString.Empty;
				}
				else
				{
					manager.CommitTransaction();
				}
			}

			return fileName;
		}

		protected virtual ZString GenerateFilename(IDbConnected connected, bool progressNumber)
		{
			return GetGenerateFileID(connected, progressNumber);
		}

		public virtual ZString GetGenerateFileID(IDbConnected connected, bool progressNumber)
		{
			return progressNumber ? New().FileID.GetNextFormatted(connected) : New().FileID.PeekPreliminaryFormatted(connected);
		}

		protected virtual long MinValue
		{
			get { return 1; }
		}

		protected virtual long MaxValue
		{
			get { return 99999; }
		}

		protected virtual ZString FountainName
		{
			get { return "FileID"; }
		}

		public INumberFountainProxy FileID
		{
			get { return new NonFormattedNumberFountainFactory(FountainName, rollOver: true, minValue: MinValue, maxValue: MaxValue).New(); }
		}
	}
}
