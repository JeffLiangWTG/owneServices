using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using CargoWise.Common.Collections;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A repository for application icons and images. You can use the an enumeration
	/// to get icons and images from here.<br/>
	/// </summary>
	/// <remarks>
	/// To edit the icons, use the 'Resourcer' freeware application. If you want to
	/// add resource files, create the file with the extention .resources and add it to the
	/// project, then pass it's resource manager to the constructor.<br/>
	/// <br/>
	/// To use this, create a sub-class like this:<br/>
	/// <br/>
	/// <c>
	/// public class MyIcons : Icons
	/// {
	/// 	protected MyIcons()
	/// 	{
	/// 	}
	/// _
	/// 	public static Icon GetIcon(MyIconTypes typeKey)
	/// 	{ return Instance.GetIconCore(typeKey); }
	/// _
	/// 	static MyIcons Instance
	/// 	{
	/// 		get { return instance ?? (instance = new MyIcons()); }
	/// 	}
	///		[ThreadStatic]
	/// 	static MyIcons instance;
	/// 	
	///     protected override ResourceManager[] NewResourceManagers()
	///     {
	///			get { return new ResourceManager[] { GetResourceManager(typeof(MyIcons).Namespace + ".MyIcons", typeof(MyIcons).Assembly) };
	///		}
	/// }
	/// </c>
	/// <br/>
	/// Where MyIconTypes is an enumeration whose names are keys in the resource manager.
	/// </remarks>
	public abstract class KIcons
	{
		/// <summary>
		/// Get the resource managers that are used to locate the icons and images.
		/// </summary>
		protected abstract ResourceManager[] NewResourceManagers();

		/// <summary>
		/// Is the given icon type an Icon object?
		/// </summary>
		protected internal bool IsIconCore(object typeKey)
		{ return GetResource(typeKey) is Icon; }

		/// <summary>
		/// Is the given icon type an Image object?
		/// </summary>
		protected internal bool IsImageCore(object typeKey)
		{ return GetResource(typeKey) is Image; }

		/// <summary>
		/// Gets an icon resource matching the given key.ToString().
		/// </summary>
		/// <param name="typeKey">The key to the icon to retrieve.</param>
		protected internal Icon GetIconCore(object typeKey)
			=> GetResource(typeKey) as Icon
				?? throw new ArgumentException("Image '" + typeKey + "' is not an icon!");

		/// <summary>
		/// Gets an image resource matching the given key.ToString().
		/// </summary>
		/// <param name="typeKey">The key to the icon to retrieve.</param>
		protected internal Image GetImageCore(object typeKey)
			=> GetResource(typeKey) as Image
				?? throw new ArgumentException("Icon '" + typeKey + "' is not an image!");

		/// <summary>
		/// Get the index into the ImageList of a given icon/image. If the icon/image doesn't
		/// already exist in the image list, it is added to it.
		/// </summary>
		protected internal int GetImageListIndexCore(object typeKey)
		{
			object index = ImageListIndexes[typeKey];
			if (index == null)
			{
				if (IsImageCore(typeKey))
				{
					ImageListCore.Images.Add(GetImageCore(typeKey));
					index = ImageListCore.Images.Count - 1;
				}
				else if (IsIconCore(typeKey))
				{
					this.ImageListCore.Images.Add(GetIconCore(typeKey).ToBitmap());
					index = this.ImageListCore.Images.Count - 1;
				}
				else
				{
					throw new InvalidOperationException();
				}
				ImageListIndexes[typeKey] = index;
			}
			return (int)index;
		}

		/// <summary>
		/// Get the ImageList the GetImageListIndex indexes into.
		/// </summary>
		protected ImageList ImageListCore
		{ get { return This.imageList ?? (This.imageList = new ImageList()); } }

		protected static ResourceManager GetResourceManager(string name, Assembly assembly)
		{
			return
				((IList)assembly.GetManifestResourceNames()).Contains(name + ".resources.resources") ?
				new ResourceManager(name + ".resources", assembly) :
				new ResourceManager(name, assembly);
		}

		#region Private Fields

		[ThreadStatic]
		static WeakReferencedKeyDictionary<KIcons, PrivateFields> fields;
		PrivateFields This
		{
			get
			{
				if (fields == null)
				{
					fields = new WeakReferencedKeyDictionary<KIcons, PrivateFields>();
				}
				PrivateFields result = fields[this];
				if (result == null)
				{
					result = new PrivateFields();
					fields[this] = result;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
		class PrivateFields
		{
			internal ResourceManager lastRM;
			internal ResourceManager[] iconRMs;
			internal Hashtable resourceCacheMap = new Hashtable();

			internal ImageList imageList = new ImageList();
			internal Hashtable imageListIndexes = new Hashtable();
		}

		#endregion

		#region Implementation

		Hashtable ImageListIndexes
		{ get { return This.imageListIndexes ?? (This.imageListIndexes = new Hashtable()); } }

		Hashtable ResourceCacheMap
		{ get { return This.resourceCacheMap ?? (This.resourceCacheMap = new Hashtable()); } }

		ResourceManager[] GetResourceManagers()
		{ return This.iconRMs ?? (This.iconRMs = NewResourceManagers()); }

		/// <summary>
		/// Gets a resource image or icon given the type. The icon or image is cached for future
		/// use and the last accessed resource manager object is scanned first for the image/icon
		/// for performance.
		/// </summary>
		object GetResource(object typeKey)
		{
			object result = ResourceCacheMap[typeKey];
			if (result == null)
			{
				if (This.lastRM != null)
				{
					result = This.lastRM.GetObject(typeKey.ToString());
				}
				if (result == null)
				{
					foreach (ResourceManager rM in GetResourceManagers())
					{
						result = rM.GetObject(typeKey.ToString());
						if (result != null)
						{
							This.lastRM = rM;
							break;
						}
					}
				}
				ResourceCacheMap[typeKey] = result;
			}
			return result;
		}

		#endregion
	}
}
