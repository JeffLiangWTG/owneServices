namespace CargoWise.Windows.UI.Interop
{
	/// <summary>
	/// Constants for WM_xxx windows messages.
	/// </summary>
	public abstract class WindowMessages
	{
		public const int WM_NULL = 0x0;
		public const int WM_PAINT = 0x000F;
		public const int WM_PRINTCLIENT = 0x0318;
		public const int WM_ERASEBKGND = 0x0014;

		public const int WM_KEYDOWN = 0x100;
		public const int WM_KEYUP = 0x101;
		public const int WM_KEYPRESS = 0x102;
		public const int WM_CHAR = 0x102;
		public const int WM_NCHITTEST = 0x0084;

		public const int WM_MOVE = 0x0003;
		public const int WM_SIZE = 0x0005;
		public const int WM_ACTIVATE = 0x0006;
		public const int WM_MOUSEACTIVATE = 0x0021;

		public const int WM_MOUSEFIRST = 0x0200;
		public const int WM_MOUSEMOVE = 0x0200;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;
		public const int WM_RBUTTONUP = 0x0205;
		public const int WM_RBUTTONDBLCLK = 0x0206;
		public const int WM_MBUTTONDOWN = 0x0207;
		public const int WM_MBUTTONUP = 0x0208;
		public const int WM_MBUTTONDBLCLK = 0x0209;
		public const int WM_MOUSEWHEEL = 0x020A;
		public const int WM_XBUTTONDOWN = 0x020B;
		public const int WM_XBUTTONUP = 0x020C;
		public const int WM_XBUTTONDBLCLK = 0x020D;
		public const int WM_MOUSELAST = 0x020A;

		public const int WM_NCMOUSEFIRST = 0x00A0;
		public const int WM_NCMOUSEMOVE = 0x00A0;
		public const int WM_NCLBUTTONDOWN = 0x00A1;
		public const int WM_NCLBUTTONUP = 0x00A2;
		public const int WM_NCLBUTTONDBLCLK = 0x00A3;
		public const int WM_NCRBUTTONDOWN = 0x00A4;
		public const int WM_NCRBUTTONUP = 0x00A5;
		public const int WM_NCRBUTTONDBLCLK = 0x00A6;
		public const int WM_NCMBUTTONDOWN = 0x00A7;
		public const int WM_NCMBUTTONUP = 0x00A8;
		public const int WM_NCMBUTTONDBLCLK = 0x00A9;
		public const int WM_NCXBUTTONDOWN = 0x00AB;
		public const int WM_NCXBUTTONUP = 0x00AC;
		public const int WM_NCXBUTTONDBLCLK = 0x00AD;
		public const int WM_NCMOUSELAST = 0x00AD;

		public const int WM_SYSCOMMAND = 0x0112;
		public const int WM_HSCROLL = 0x0114;
		public const int WM_VSCROLL = 0x0115;
		public const int WM_CONTEXTMENU = 0x007B;

		public const int CB_GETDROPPEDWIDTH = 0x015f;
		public const int CB_SETDROPPEDWIDTH = 0x0160;

		public const int EM_GETCUEBANNER = 0x1502;
		public const int EM_SETCUEBANNER = 0x1501;
	}
}
